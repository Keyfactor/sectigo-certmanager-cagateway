using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SectigoSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string[]> syncFilter = new Dictionary<string, string[]>
            {
                //["sslTypeId"]=new string[] { "14078","14079","14065"},
                //["orgId"] =new string[] { "101","102"}
            };

            string filterQueryString = string.Empty;            
            foreach (var f in syncFilter)
            {
                filterQueryString += $"{f.Key}={String.Join($"&{f.Key}=", f.Value)}&";
            }

            filterQueryString = filterQueryString.TrimEnd('&');


            string pemChain = System.IO.File.ReadAllText(@"C:\Users\gnoe\Desktop\509_certonly.txt");
            //X509Certificate2Collection collection = new X509Certificate2Collection();
            //collection.Import(System.IO.File.ReadAllBytes(@"C:\Users\gnoe\Desktop\pkcs7.txt"));
            //X509Certificate cert = X509Certificate.CreateFromCertFile(@"C:\Users\gnoe\Desktop\pkcs7.txt");

            string[] splitChain = pemChain.Replace("\r\n", string.Empty).Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries);

            X509Certificate2 newcert = new X509Certificate2(Convert.FromBase64String(splitChain[1]));
            X509Certificate2Collection certChain = new X509Certificate2Collection();
            for (int i = 1; i < splitChain.Length; i += 3)
            {
                certChain.Add(new X509Certificate2(Convert.FromBase64String(splitChain[i])));
            }
            X509Certificate2Collection foundCerts = certChain.Find(X509FindType.FindBySubjectName, "apitest2.pkiops.com", false);

        }
    }
}
