using CAProxy.AnyGateway.Interfaces;
using CAProxy.AnyGateway.Models;
using CAProxy.Data.CAProxyDAL.Esent;
using CSS.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Keyfactor.AnyGateway.Sectigo.Database
{
    public class SectigoEsentMigrator : LoggingClientBase, IDatabaseMigrator
    {
        public void GetAllCertificates(BlockingCollection<DBCertificate> certificateBuffer, DBCertificateAuthority certificateAuthority, string esentDatabasePath)
        {
            const string ESENT_PATH = "C:\\ProgramData\\CertSvcProxy\\TemplateDB\\CAProxyComodoCCM.edb";
            try
            {
                if (!string.IsNullOrEmpty(esentDatabasePath))
                {
                    Logger.Warn($"--esent-path parameter is not supported.  Using default {ESENT_PATH}");
                }

                Logger.Trace("Getting All Certificates");
                if (File.Exists(ESENT_PATH))
                {
                    ComodoCcmESENTConnector conn = new ComodoCcmESENTConnector();
                    foreach (var record in conn.GetAllRecords())
                    {
                        var arrayCert = record.Certificate?.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries);
                        var arrayCsr = record.CSR?.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries);

                        string pemCert = string.Empty;
                        if (arrayCert?.Length > 1)
                            pemCert = arrayCert[1].Replace("\n", string.Empty);

                        string pemCsr = string.Empty;
                        if (arrayCsr?.Length > 1)
                            pemCsr = arrayCsr[1].Replace("\n", string.Empty);

                        DBCertificate dbCert = new DBCertificate
                        {
                            CARequestID = $"{record.RequestID}",
                            Template = record.CertificateTemplate,
                            SubmissionDate = record.SubmitDate, // Default input for migration. Will be updated on sync. Date must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
                            Status = 0, // Default input for migration. Will be updated on sync
                            CertificateAuthorityId = certificateAuthority.Id,
                            Certificate = Convert.FromBase64String(pemCert),
                            CSR = Convert.FromBase64String(pemCsr),
                            Requester = record.Requestor,
                            IssuedCN = record.CommonName
                        };
                        certificateBuffer.Add(dbCert);

                    }
                }
                else
                {
                    throw new Exception($"ESENT DB at C:\\ProgramData\\CertSvcProxy\\TemplateDB\\CAProxyComodoCCM.edb not found. Migration failed.");
                }
            }
            catch (NullReferenceException nullex)
            {
                Logger.Error(nullex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }
    }
}
