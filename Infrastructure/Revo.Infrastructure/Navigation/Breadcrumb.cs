using System;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Navigation
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
