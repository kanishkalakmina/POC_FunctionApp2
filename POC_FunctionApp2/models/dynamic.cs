using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*{
    "id": "2323"
    "type": "CustomFieldCollection",
    "mappedFor": "Resource",
    "Title": "Doctor", // name of the resource
    "CustomFields": [
        {
            "name": "Qualification",
            "dataType": "string",
            "order": 1,
            "rules": [
                {
                    "type": "MaxLength",
                    "arguments": 50
                },
                {
                    "type": "MinLength",
                    "arguments": 5
                },
                {
                    "type": "Required",
                    "arguments": null
                }
            ]
        }
    ]
}*/

namespace POC_FunctionApp2.models
{
    public class CustomFieldCollection 
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public List<CustomFields> CustomField { get; set; }

        public void Update(CustomFieldCollection newCustomFieldCollection)
        {
            this.CustomField = newCustomFieldCollection.CustomField;
        }
    }
    
    public class CustomFields
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public int Order { get; set; }
        public List<CustomFieldRules> Rule { get; set; }

        public void Update(CustomFields newCustomFields)
        {
            this.Name = newCustomFields.Name;
            this.DataType = newCustomFields.DataType;
            this.Order = newCustomFields.Order;
            this.Rule = newCustomFields.Rule;
        }
    }

    public class CustomFieldRules
    {
        public string Type { get; set; }
        public object Argument { get; set; }

        public void Update(CustomFieldRules newCustomFieldRules)
        {
            this.Type = newCustomFieldRules.Type;
            this.Argument = newCustomFieldRules.Argument;
        }
    }
}
