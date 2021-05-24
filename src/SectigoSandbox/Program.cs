using CAProxy.AnyGateway.Models;
using Keyfactor.AnyGateway.Sectigo.Database;
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
            SectigoEsentMigrator migrator = new SectigoEsentMigrator();

            migrator.GetAllCertificates(new System.Collections.Concurrent.BlockingCollection<CAProxy.AnyGateway.Models.DBCertificate>(), 
                new DBCertificateAuthority(),
                "C:\\temp\\test");
        }
    }
}
