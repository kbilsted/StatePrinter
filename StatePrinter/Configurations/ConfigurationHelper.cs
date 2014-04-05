using System.Globalization;
using StatePrinter.FieldHarvest;
using StatePrinter.FieldHarvesters;
using StatePrinter.OutputFormatters;
using StatePrinter.ValueConverters;

namespace StatePrinter.Configurations
{
  public static class ConfigurationHelper
  {

    /// <summary>
    /// Return a configuration which covers most usages.
    /// The configuration returned can be further remolded by adding additional handlers. 
    /// 
    /// Eg. add a <see cref="PublicFieldsHarvester"/> to restrict the printed state to only public fields.
    /// </summary>
    public static Configuration GetStandardConfiguration()
    {
      return GetStandardConfiguration(CultureInfo.CurrentCulture);
    } 

    /// <summary>
    /// Return a configuration which covers most usages.
    /// The configuration returned can be further remolded by adding additional handlers. 
    /// 
    /// Eg. add a <see cref="PublicFieldsHarvester"/> to restrict the printed state to only public fields.
    /// </summary>
    public static Configuration GetStandardConfiguration(CultureInfo culture)
    {
      var cfg = new Configuration
                {
                  Culture = culture
                };

      // valueconverters
      cfg.Add(new StandardTypesConverter(cfg.Culture));
      cfg.Add(new StringConverter());
      cfg.Add(new DateTimeConverter(cfg.Culture));
      cfg.Add(new EnumConverter());
      
      // harvesters
      cfg.Add(new AllFieldsHarvester());

      // outputformatters
      cfg.OutputFormatter = new CurlyBraceStyle(cfg.IndentIncrement);
      
      return cfg;
    }
  }
}