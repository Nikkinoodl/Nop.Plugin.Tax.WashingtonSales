using System;
using System.IO;
using System.Web.Routing;
using System.Web;
using System.Net;
using System.Xml;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tax;

namespace Nop.Plugin.Tax.WashingtonSales
{
    /// <summary>
    /// Washington sales tax provider
    /// </summary>
    public class WashingtonSalesTaxProvider : BasePlugin, ITaxProvider
    {
        private readonly ISettingService _settingService;

        public WashingtonSalesTaxProvider(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult();
            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            string address1 = calculateTaxRequest.Address.Address1;
            string address2 = calculateTaxRequest.Address.Address2;
            string city = calculateTaxRequest.Address.City;
            string zip = calculateTaxRequest.Address.ZipPostalCode;
            string stateProvince = calculateTaxRequest.Address.StateProvince.Abbreviation;
            string country = calculateTaxRequest.Address.Country.TwoLetterIsoCode;

            //WA has destination based tax based on delivery address so return error if address line1
            //or zip is missing. 
            if (String.IsNullOrWhiteSpace(address1) || String.IsNullOrWhiteSpace(zip))
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            //Format string for calling Washington State Dept of Revenue Service
            string protocol = "http";
            string server = "dor.wa.gov";

            string urlPrefix = protocol +

                "://" + server + ":" +

                "/AddressRates.aspx?output=xml";

            string uri = urlPrefix +

                          "&addr=" +

                          HttpUtility.UrlEncode(address1) +

                          " " +
                          HttpUtility.UrlEncode(address2) +

                          "&city=" +

                          HttpUtility.UrlEncode(city) +

                          "&zip=" +

                          zip;

            //Make the call to the web service and read the response
            System.Net.WebClient wc = new WebClient();
            StreamReader reader = new StreamReader(wc.OpenRead(uri));
            string xml = reader.ReadToEnd();
            reader.Close();

            string xmlLocation;
            string xmlLocalRate;
            string xmlRate;
            string xmlErrorCode;

            //Read the returned string to XML
            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml)))
            {
                xmlReader.ReadToFollowing("response");
                xmlReader.MoveToAttribute("code");
                xmlErrorCode = xmlReader.Value;
                xmlReader.MoveToAttribute("rate");
                xmlRate = xmlReader.Value;
                xmlReader.MoveToAttribute("localrate");
                xmlLocalRate = xmlReader.Value;
                xmlReader.MoveToAttribute("loccode");
                xmlLocation = xmlReader.Value;
            }

            //Codes returned with a rate: 0=address found, 1=address not found but zip+4 OK
            //Codes returned if no rate : 2=address not found zip+4 not found, 3=address and zip code not found
            //4=invalid arguments, 5=internal error

            int errorCode = Convert.ToInt16(xmlErrorCode);

            // Multiply the given rate by 100 and convert to decimal
            if ((errorCode == 0 || errorCode == 1) && xmlRate != null)
            {
                double rate = double.Parse(xmlRate);
                rate = rate * 100;
                result.TaxRate = Convert.ToDecimal(rate);
                return result;
            }

            //Set tax rate to 0 for all out of state deliveries
            //Note that we have looked up address in WA state system to make sure this isn't a WA
            //address with the state set incorrectly
            if ((!string.Equals(stateProvince, "WA")) && (errorCode == 3))
            {
                result.TaxRate = 0.00M;
                return result;
            }
            else
            {
                result.Errors.Add("Unable to obtain rate.");
                return result;
            }
        }

        /// <summary>
        /// Gets a route for provider configuration. This returns NULL as this plugin is not configurable.
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">TaxWashingtonSalesController</param>
        /// <param name="routeValues">null</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxWashingtonSales";
            routeValues = new RouteValueDictionary
            {
                { "Namespaces", "Nop.Plugin.Tax.WashingtonSales.Controllers" },
                { "area", null }
            };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.WashingtonSales.Fields.TaxCategoryName", "Tax category");
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.WashingtonSales.Fields.TaxCategoryName");
            base.Uninstall();
        }
    }
}
