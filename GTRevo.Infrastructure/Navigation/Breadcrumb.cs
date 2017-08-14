using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Navigation
{
    public class Breadcrumb
    {
        public Breadcrumb(string label, Guid classId, string code)
        {
            Label = label;
            ClassId = classId;
            Code = code;
        }

        public Breadcrumb(string label, Type type, string code)
        {
            Label = label;
            ClassId = type.GetClassId();
            Code = code;
        }

        protected Breadcrumb()
        {
        }

        public string Label { get; private set; }
        public Guid ClassId { get; private set; }
        public string Code { get; private set; }
    }
}
