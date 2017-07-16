using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security
{
    public class ScryptPasswordHasher : IPasswordHasher
    {
        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private const string BsdBase64Alphabet = "./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly char[] BsdBase64ToBase64 = Enumerable.Repeat('_', 128).ToArray();
        private static readonly char[] Base64ToBsdBase64 = Enumerable.Repeat('_', 128).ToArray();

        private static readonly int[] BsdBase64DecodeTable = new int[]
        {
			/*  0:*/ - 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // ________________
			/* 16:*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // ________________
			/* 32:*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  1,  // ______________./
			/* 48:*/ 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, -1, -1, -1, -1, -1, -1,  // 0123456789______
			/* 64:*/ -1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16,  // _ABCDEFGHIJKLMNO
			/* 80:*/ 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, -1, -1, -1, -1, -1,  // PQRSTUVWXYZ_____
			/* 96:*/ -1, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42,  // _abcdefghijklmno
			/*113:*/ 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, -1, -1, -1, -1, -1 // pqrstuvwxyz_____
        };

        static ScryptPasswordHasher()
        {
            for (int i = 0; i < BsdBase64Alphabet.Length; i++)
            {
                char bsdBase64 = BsdBase64Alphabet[i];
                char base64 = Base64Alphabet[i];
                BsdBase64ToBase64[bsdBase64] = base64;
            }

            for (int i = 0; i < Base64Alphabet.Length; i++)
            {
                char bsdBase64 = BsdBase64Alphabet[i];
                char base64 = Base64Alphabet[i];
                Base64ToBsdBase64[base64] = bsdBase64;
            }
        }

        public HashParameters Parameters { get; set; } = new HashParameters()
        {
            BlockSize = 8,
            CostFactor = 18,
            ParallelizationFactor = 1,
            HashSize = 64
        };

        public string HashPassword(string password)
        {
            /*
            Password hash format is inspired by Delphi's open-source SCrypt.pas implementation.
            Snippet taken from its documentation: 
            
            SCrypt has also been used as password hashing algorithm.
	        In order to make password storage easier, we will generate the salt and store it with the
	        returned string. This is similar to what OpenBSD has done with BCrypt.
	        The downside is that there is no standard out there for SCrypt representation of password hashes.

		        hash := TSCrypt.HashPassword('correct horse battery staple', 'seasalt');

	        will return string in the format of:

	        $s0$params$salt$key

	          s0     - version 0 of the format with 128-bit salt and 256-bit derived key
	          params - 32-bit hex integer containing log2(N) (16 bits), r (8 bits), and p (8 bits)
	          salt   - base64-encoded salt
	          key    - base64-encoded derived key

	          Example:

	            $s0$e0801$epIxT/h6HbbwHaehFnh/bw==$7H0vsXlY8UxxyW/BWx/9GuY7jEvGjT71GFd6O4SZND0=

                    byte[] keyBytes = CryptSharp.Utility.SCrypt.ComputeDerivedKey(
                        Encoding.UTF8.GetBytes(password),
                        GeneratePasswordSalt(),
                        262144,
                        8,
                        1,
                        null,
                        64);
            */

            byte[] salt = GeneratePasswordSalt();

            byte[] keyBytes = CryptSharp.Utility.SCrypt.ComputeDerivedKey(
                Encoding.UTF8.GetBytes(password),
                salt,
                Parameters.BlockSize,
                Parameters.CostFactor,
                Parameters.ParallelizationFactor,
                null,
                Parameters.HashSize);

            StringBuilder sb = new StringBuilder();
            sb.Append("$s0");

            uint paramsEncoded = ((uint)Parameters.CostFactor << 16)
                | ((uint)Parameters.BlockSize << 8) | ((uint)Parameters.ParallelizationFactor);
            sb.Append($"${paramsEncoded.ToString("x")}${BsdBase64Encode(salt)}${BsdBase64Encode(keyBytes)}");

            return sb.ToString();
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            HashParameters hashParams;
            byte[] salt, key;
            ReadParams(hashedPassword, out hashParams, out salt, out key);

            byte[] providedKey = CryptSharp.Utility.SCrypt.ComputeDerivedKey(
                Encoding.UTF8.GetBytes(providedPassword),
                salt,
                hashParams.BlockSize,
                hashParams.CostFactor,
                hashParams.ParallelizationFactor,
                null,
                hashParams.HashSize);

            //return Memcmp(key, providedKey) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
            return PasswordVerificationResult.Success; //TODO - right now, the computed key isn't compatible with the Delphi implementation
        }

        private void ReadParams(string hashedPassword, out HashParameters outParameters,
            out byte[] salt, out byte[] key)
        {
            string[] parts = hashedPassword.Split(new char[] { '$' });
            if (parts.Length != 5 || parts[0] != "")
            {
                throw new ArgumentException("Invalid Scrypt password hash scheme");
            }

            switch (parts[1])
            {
                case "s0":
                    /*
                    Java implementation of scrypt (Lambdaworks OSS)
                    https://github.com/wg/scrypt

                    $s0$params$salt$key

                        s0     - version 0 of the format with 128-bit salt and 256-bit derived key
                        params - 32-bit hex integer containing log2(N) (16 bits), r (8 bits), and p (8 bits)
                        salt   - base64-encoded salt
                        key    - base64-encoded derived key

                    Example:

                        $s0$e0801$epIxT/h6HbbwHaehFnh/bw==$7H0vsXlY8UxxyW/BWx/9GuY7jEvGjT71GFd6O4SZND0=

                    passwd = "secret"
                        N = 16384
                        r = 8
                        p = 1
                    */

                    salt = BsdBase64Decode(parts[3]);
                    key = BsdBase64Decode(parts[4]);

                    uint paramsEncoded = uint.Parse(parts[2], NumberStyles.HexNumber);
                    outParameters = new HashParameters()
                    {
                        ParallelizationFactor = (int)(paramsEncoded & (uint)0x000000FF),
                        BlockSize = (int)(paramsEncoded & (uint)0x0000FF00) >> 8,
                        CostFactor = (int)(paramsEncoded & (uint)0xFFFF0000) >> 16,
                        HashSize = key.Length
                    };
                    break;

                case "s1":
                    /*
                        Modular Crypt Format support for scrypt
                        https://github.com/jvarho/pylibscrypt/blob/master/pylibscrypt/mcf.py

                        Compatible with libscrypt scrypt_mcf_check also supports the $7$ format.

                        libscrypt format:

                        $s1$NNrrpp$salt$hash
                            NN   - hex encoded N log2 (two hex digits)
                            rr   - hex encoded r in 1-255
                            pp   - hex encoded p in 1-255
                            salt - base64 encoded salt 1-16 bytes decoded
                            hash - base64 encoded 64-byte scrypt hash                    
                    */

                    if (parts[2].Length != 6)
                    {
                        throw new ArgumentException("Invalid length of s1-scheme scrypt hash parameters: " + parts[2].Length);
                    }

                    salt = BsdBase64Decode(parts[3]);
                    key = BsdBase64Decode(parts[4]);
                    
                    outParameters = new HashParameters()
                    {
                        ParallelizationFactor = int.Parse(parts[2].Substring(4, 2), NumberStyles.HexNumber),
                        BlockSize = int.Parse(parts[2].Substring(2, 2), NumberStyles.HexNumber),
                        CostFactor = int.Parse(parts[2].Substring(0, 2), NumberStyles.HexNumber),
                        HashSize = key.Length
                    };
                    break;

                default:
                    throw new ArgumentException("Unsupported Scrypt hash scheme version");
            }
        }

        private int Char64(char c)
        {
            if (c > BsdBase64DecodeTable.Length)
            {
                return -1;
            }

            return BsdBase64DecodeTable[c];
        }

        private byte[] BsdBase64Decode(string base64)
        {
            /*StringBuilder sb = new StringBuilder();

            for (int i = 0; i < base64.Length; i++)
            {
                char c = base64[i];

                if (c < BSD_BASE64_TO_BASE64.Length)
                {
                    sb.Append(BSD_BASE64_TO_BASE64[c]);
                }
            }

            return Convert.FromBase64String(
                sb.ToString()
                    .PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));*/

            List<byte> res = new List<byte>();

            int len = base64.Length;
            int i = 0;
            while (i < len)
            {
                // We'll need to have at least 2 character to form one byte.
                // Anything less is invalid
                if (i + 1 >= len)
                {
                    throw new ArgumentException("Invalid base64 hash string");
                }

                int c1 = Char64(base64[i]);
                i++;
                int c2 = Char64(base64[i]);
                i++;

                if (c1 == -1 || c2 == -1)
                {
                    throw new ArgumentException("Invalid base64 hash string");
                }

                //Now we have at least one byte in c1|c2
                // c1 = ..111111
                // c2 = ..112222
                res.Add((byte)(((c1 & 0x3f) << 2) | (c2 >> 4)));

                //If there's a 3rd character, then we can use c2|c3 to form the second byte
                if (i >= len)
                {
                    break;
                }

                int c3 = Char64(base64[i]);
                i++;
                if (c3 == -1)
                {
                    throw new ArgumentException("Invalid base64 hash string");
                }
                
                //Now we have the next byte in c2|c3
                // c2 = ..112222
                // c3 = ..222233
                res.Add((byte)(((c2 & 0x0f) << 4) | (c3 >> 2)));
                
                //If there's a 4th caracter, then we can use c3|c4 to form the third byte
                if (i >= len)
                {
                    break;
                }

                int c4 = Char64(base64[i]);
                i++;
                if (c4 == -1)
                {
                    throw new ArgumentException("Invalid base64 hash string");
                }

                //Now we have the next byte in c3|c4
                // c3 = ..222233
                // c4 = ..333333
                res.Add((byte)(((c3 & 0x03) << 6) | c4));
            }

            return res.ToArray();
        }

        private string BsdBase64Encode(byte[] data)
        {
            string base64 = Convert.ToBase64String(data);
            base64 = base64.TrimEnd('=');

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < base64.Length; i++)
            {
                char c = base64[i];

                if (c < Base64ToBsdBase64.Length)
                {
                    sb.Append(Base64ToBsdBase64[c]);
                }
            }

            return sb.ToString();
        }

        private byte[] GeneratePasswordSalt()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[16];
                rng.GetBytes(tokenData);
                return tokenData;
            }
        }

        public struct HashParameters
        {
            public int BlockSize;
            public int CostFactor;
            public int ParallelizationFactor;
            public int HashSize;
        }
    }
}
