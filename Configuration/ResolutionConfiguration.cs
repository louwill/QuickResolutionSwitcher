using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickResolutionSwitcher.Configuration
{
    #region === ResolutionConfigurationSection ===

    public class ResolutionConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ResolutionConfigurationElementCollection Items
        {
            get { return this[""] as ResolutionConfigurationElementCollection; }
            set { this[""] = value; }
        }
    }

    #endregion


    #region === ResolutionConfigurationElementCollection ===

    [ConfigurationCollection(typeof(ResolutionConfigurationElement), AddItemName = "resolution", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ResolutionConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ResolutionConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ResolutionConfigurationElement)element).Value;
        }

        public ResolutionConfigurationElement this[int index]
        {
            get { return (ResolutionConfigurationElement)BaseGet(index); }
        }
    }

    #endregion


    #region === ResolutionConfigurationElement ===

    public class ResolutionConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }

    #endregion
}
