using Newtonsoft.Json;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class CurrencyDAL
    {
        public bool verifyCurrency(string Currency)
        {
            String URLString = "https://v6.exchangerate-api.com/v6/2c960b6115b653355438d4bb/latest/" + Currency;
            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    var json = webClient.DownloadString(URLString);
                    ExchangeRateResponse Test = JsonConvert.DeserializeObject<ExchangeRateResponse>(json);
                    if (Test.result == "error")
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public double convertCurrency(double value, string fromCurrency, string toCurrency)
        {
            String URLString = "https://v6.exchangerate-api.com/v6/2c960b6115b653355438d4bb/latest/" + fromCurrency;
            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(URLString);
                ExchangeRateResponse Test = JsonConvert.DeserializeObject<ExchangeRateResponse>(json);
                if (toCurrency == "AED")
                {
                    value = value * Test.conversion_rates.AED;
                }
                else if (toCurrency == "ARS")
                {
                    value = value * Test.conversion_rates.ARS;
                }
                else if (toCurrency == "AUD")
                {
                    value = value * Test.conversion_rates.AUD;
                }
                else if (toCurrency == "BGN")
                {
                    value = value * Test.conversion_rates.BGN;
                }
                else if (toCurrency == "BRL")
                {
                    value = value * Test.conversion_rates.BRL;
                }
                else if (toCurrency == "BSD")
                {
                    value = value * Test.conversion_rates.BSD;
                }
                else if (toCurrency == "CAD")
                {
                    value = value * Test.conversion_rates.CAD;
                }
                else if (toCurrency == "CHF")
                {
                    value = value * Test.conversion_rates.CHF;
                }
                else if (toCurrency == "CLP")
                {
                    value = value * Test.conversion_rates.CLP;
                }
                else if (toCurrency == "CNY")
                {
                    value = value * Test.conversion_rates.CNY;
                }
                else if (toCurrency == "COP")
                {
                    value = value * Test.conversion_rates.COP;
                }
                else if (toCurrency == "CZK")
                {
                    value = value * Test.conversion_rates.CZK;
                }
                else if (toCurrency == "DKK")
                {
                    value = value * Test.conversion_rates.DKK;
                }
                else if (toCurrency == "DOP")
                {
                    value = value * Test.conversion_rates.DOP;
                }
                else if (toCurrency == "EGP")
                {
                    value = value * Test.conversion_rates.EGP;
                }
                else if (toCurrency == "EUR")
                {
                    value = value * Test.conversion_rates.EUR;
                }
                else if (toCurrency == "FJD")
                {
                    value = value * Test.conversion_rates.FJD;
                }
                else if (toCurrency == "GBP")
                {
                    value = value * Test.conversion_rates.GBP;
                }
                else if (toCurrency == "HKD")
                {
                    value = value * Test.conversion_rates.HKD;
                }
                else if (toCurrency == "HRK")
                {
                    value = value * Test.conversion_rates.HRK;
                }
                else if (toCurrency == "HUF")
                {
                    value = value * Test.conversion_rates.HUF;
                }
                else if (toCurrency == "IDR")
                {
                    value = value * Test.conversion_rates.IDR;
                }
                else if (toCurrency == "ILS")
                {
                    value = value * Test.conversion_rates.ILS;
                }
                else if (toCurrency == "INR")
                {
                    value = value * Test.conversion_rates.INR;
                }
                else if (toCurrency == "ISK")
                {
                    value = value * Test.conversion_rates.ISK;
                }
                else if (toCurrency == "JPY")
                {
                    value = value * Test.conversion_rates.JPY;
                }
                else if (toCurrency == "KRW")
                {
                    value = value * Test.conversion_rates.KRW;
                }
                else if (toCurrency == "KZT")
                {
                    value = value * Test.conversion_rates.KZT;
                }
                else if (toCurrency == "MXN")
                {
                    value = value * Test.conversion_rates.MXN;
                }
                else if (toCurrency == "MYR")
                {
                    value = value * Test.conversion_rates.MYR;
                }
                else if (toCurrency == "NOK")
                {
                    value = value * Test.conversion_rates.NOK;
                }
                else if (toCurrency == "NZD")
                {
                    value = value * Test.conversion_rates.NZD;
                }
                else if (toCurrency == "PAB")
                {
                    value = value * Test.conversion_rates.PAB;
                }
                else if (toCurrency == "PEN")
                {
                    value = value * Test.conversion_rates.PEN;
                }
                else if (toCurrency == "PHP")
                {
                    value = value * Test.conversion_rates.PHP;
                }
                else if (toCurrency == "PKR")
                {
                    value = value * Test.conversion_rates.PKR;
                }
                else if (toCurrency == "PLN")
                {
                    value = value * Test.conversion_rates.PLN;
                }
                else if (toCurrency == "PYG")
                {
                    value = value * Test.conversion_rates.PYG;
                }
                else if (toCurrency == "RON")
                {
                    value = value * Test.conversion_rates.RON;
                }
                else if (toCurrency == "RUB")
                {
                    value = value * Test.conversion_rates.RUB;
                }
                else if (toCurrency == "SAR")
                {
                    value = value * Test.conversion_rates.SAR;
                }
                else if (toCurrency == "SEK")
                {
                    value = value * Test.conversion_rates.SEK;
                }
                else if (toCurrency == "SGD")
                {
                    value = value * Test.conversion_rates.SGD;
                }
                else if (toCurrency == "THB")
                {
                    value = value * Test.conversion_rates.THB;
                }
                else if (toCurrency == "TRY")
                {
                    value = value * Test.conversion_rates.TRY;
                }
                else if (toCurrency == "TWD")
                {
                    value = value * Test.conversion_rates.TWD;
                }
                else if (toCurrency == "UAH")
                {
                    value = value * Test.conversion_rates.UAH;
                }
                else if (toCurrency == "USD")
                {
                    value = value * Test.conversion_rates.USD;
                }
                else if (toCurrency == "UYU")
                {
                    value = value * Test.conversion_rates.UYU;
                }
                else if (toCurrency == "ZAR")
                {
                    value = value * Test.conversion_rates.ZAR;
                }
                else if (toCurrency == null)
                {
                    value = value * Test.conversion_rates.SGD;
                }
                return value;
            }
        }
    }
}
