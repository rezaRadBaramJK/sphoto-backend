using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Baramjk.ContactUs.Domains;
using Nop.Plugin.Baramjk.ContactUs.Plugin;

namespace Nop.Plugin.Baramjk.ContactUs.Data.Mapping
{
    public class NameCompatibility: INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            {typeof(SubjectEntity), "Baramjk_ContactUs_Subjects"},
            {typeof(ContactInfoEntity), DefaultValues.ContactUsTableName },
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}