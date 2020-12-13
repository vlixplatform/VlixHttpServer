using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;
//using Certes;
//using Certes.Acme;

namespace  Vlix.HttpServer
{
    
    public class SSLCertificateServices
    {

        public static Task<List<X509Certificate2>> GetSSLCertificates(StoreName storeName)
        {
            X509Store store = new X509Store(storeName, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            List<X509Certificate2> output = new List<X509Certificate2>();
            foreach (X509Certificate2 certificate in store.Certificates) output.Add(certificate);
            return Task.FromResult(output);
        }



        public static void RemoveSSLCertFromPort(string IP, int PortNumber, Action<string> OnInfoLog = null)
        {
            
            //OnInfoLog?.Invoke("Checking if any SSL Cert has been binded to " + IP + ":" + PortNumber + "...");
            var SSLCertCheckBindingCmdRes = ExecuteCommand("netsh http show sslcert " + IP +":" + PortNumber);
            if (SSLCertCheckBindingCmdRes.IndexOf("application", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string SSLCertDeleteCmd = "netsh http delete sslcert ipport=" + IP + ":" + PortNumber;
                OnInfoLog?.Invoke("Deleting SSL Cert binded Port via Windows Command: " + SSLCertDeleteCmd);
                string BindResultText = ExecuteCommand(SSLCertDeleteCmd).RemoveAllNewLines().Trim(' ');
                OnInfoLog?.Invoke(BindResultText);
            }
        }
        
        public async static Task<bool> TryBindSSLCertToPort(int PortNumber, string subject, StoreName storeName = StoreName.My, Action<string> OnInfoLog = null, Action<string> OnErrorLog = null, bool RemoveAnyPreviousBinding = true, string IP = "0.0.0.0")
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                OnErrorLog?.Invoke("Unable to bind SSL Cert to Port as no certificate subject was specified.");
                return false;
            }
            //List<X509Certificate2> x509Certificates = new List<X509Certificate2>(); 
            var certs = await SSLCertificateServices.GetSSLCertificates(storeName);
            X509Certificate2 x509Certificate = certs.Where(c => c.HasPrivateKey && c.Subject ==subject).OrderByDescending(c=>c.NotAfter).FirstOrDefault();
            if (x509Certificate == null)
            {
                OnErrorLog?.Invoke("Unable to find SSL Certificate with subject '" + subject + "' in certificte store '" + storeName.ToString() + "'");
                return false;
            }
            
            string applicationId = null;
            var asm = Assembly.GetEntryAssembly();
            if (asm == null)  applicationId = Guid.NewGuid().ToString();
            else
            {
                try
                {
                    applicationId = ((GuidAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
                }
                catch
                { }
            }
            if (applicationId == null) applicationId = Guid.NewGuid().ToString();


            //Remove any Previously Binded SSL Sert at PORT
            if (RemoveAnyPreviousBinding) RemoveSSLCertFromPort(IP,PortNumber, (log) => OnInfoLog?.Invoke(log));

            try
            {
                string BindCommand = "netsh http add sslcert ipport=" + IP + ":" + PortNumber + " certhash=" + x509Certificate.Thumbprint + " appid={" + applicationId + "}";
                OnInfoLog?.Invoke("Binding SSL Certificate '" + subject + "' to " + IP + ":" + PortNumber); // + " via Command=" + BindCommand)
                string BindResultText = ExecuteCommand(BindCommand).RemoveAllNewLines().Trim(' ');
                OnInfoLog?.Invoke(BindResultText);
            }
            catch (Exception ex)
            {
                OnErrorLog?.Invoke("Unable to bind generate SSL Certificate to Port " + PortNumber + "\r\n" + ex.ToString());
                return false;
            }
            return true;
        }




        //public static bool TryGenerateSSLCertAndBindToPort(string SSLCertName, int PortNumber, out string ExStr, string CertSubjectName = "localhost", string[] SubjectAlternativeNames_ForHttpSSLCert = null)
        //{
        //    ExStr = "";
        //    if (SubjectAlternativeNames_ForHttpSSLCert == null) SubjectAlternativeNames_ForHttpSSLCert = new string[] { "localhost", "127.0.0.1" };
        //    Logger.Log("Obtaining Application ID...");
        //    string applicationId = null;
        //    try
        //    {
        //        applicationId = ((GuidAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
        //    }
        //    catch
        //    { }
        //    if (applicationId == null) applicationId = Guid.NewGuid().ToString();


        //    X509Certificate2 SSLCertificate = FindExistingCert_CreateCAIfNotExist(SSLCertName, SubjectAlternativeNames_ForHttpSSLCert, out AsymmetricKeyParameter CAPrivateKey);


        //    //Remove any Previously Binded SSL Sert at PORT
        //    Logger.Log("Checking if any SSL Cert has been binded to port " + PortNumber + "...");
        //    var SSLCertCheckBindingCmdRes = ExecuteCommand("netsh http show sslcert 0.0.0.0:" + PortNumber);
        //    if (SSLCertCheckBindingCmdRes.IndexOf("application", StringComparison.OrdinalIgnoreCase) >= 0)
        //    {
        //        string SSLCertDeleteCmd = "netsh http delete sslcert ipport=0.0.0.0:" + PortNumber;
        //        Logger.Log("Deleting binded Port... via Windows Command: " + SSLCertDeleteCmd);
        //        string BindResultText = ExecuteCommand(SSLCertDeleteCmd).RemoveAllNewLines().Trim(' ');
        //        Logger.Log(BindResultText);
        //    }


        //    //Generate New SSL Certificate if SSL Certificate not found
        //    if (SSLCertificate == null)
        //    {
        //        Logger.Log("Creating certificate...");
        //        try
        //        {
        //            //GENERATE BOUNCY CASTLE x509 CERTIFICATE
        //            Logger.Log("Creating certificate based on CA...");
        //            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();


        //            //Generate Assymmetric Key Pair
        //            const int keyStrength = 2048;
        //            SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
        //            var keyPairGenerator = new RsaKeyPairGenerator();
        //            keyPairGenerator.Init(new KeyGenerationParameters(random, keyStrength));
        //            AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();


        //            //Set Details
        //            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth)); //Add Enhanced Key Usage - Server Authentication (1.3.6.1.5.5.7.3.1)
        //            Asn1EncodableVector generalNames = new Asn1EncodableVector();
        //            foreach (string SAN in SubjectAlternativeNames_ForHttpSSLCert)
        //            {
        //                if (SAN.IsValidIPAddress()) generalNames.Add(new GeneralName(GeneralName.IPAddress, SAN));
        //                else generalNames.Add(new GeneralName(GeneralName.DnsName, SAN));
        //            }
        //            certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false, new DerSequence(generalNames));
        //            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
        //            certificateGenerator.SetSerialNumber(serialNumber);
        //            certificateGenerator.SetSubjectDN(new X509Name("CN=" + CertSubjectName));
        //            certificateGenerator.SetIssuerDN(new X509Name("CN=" + SSLCertName + " CA"));
        //            DateTime notBefore = DateTime.Now.AddYears(-1);
        //            certificateGenerator.SetNotBefore(notBefore);
        //            certificateGenerator.SetNotAfter(notBefore.AddYears(8));
        //            certificateGenerator.SetPublicKey(subjectKeyPair.Public); //Set Public Key                

        //            //Generate Bouncy Castle Certificate
        //            Org.BouncyCastle.X509.X509Certificate BCCertificate = certificateGenerator.Generate(new Asn1SignatureFactory("SHA512WITHRSA", CAPrivateKey, random));


        //            //GENERATE MICROSOFT X509Certificate2
        //            X509Certificate2 x509 = new X509Certificate2(BCCertificate.GetEncoded());
        //            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);
        //            Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
        //            RsaPrivateKeyStructure rsa = RsaPrivateKeyStructure.GetInstance(seq); //new RsaPrivateKeyStructure(seq);
        //            RsaPrivateCrtKeyParameters rsaparams = new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);
        //            //x509.FriendlyName = "Vlix";
        //            x509.FriendlyName = SSLCertName;
        //            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
        //            byte[] certBytes = x509.Export(X509ContentType.Pkcs12, "Vlix");
        //            SSLCertificate = new X509Certificate2(certBytes, "Vlix", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        //            X509Store Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        //            Store.Open(OpenFlags.MaxAllowed);
        //            Store.Add(SSLCertificate);
        //            Store.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            ExStr = "Unable to generate SSL Certificate\r\n" + ex.ToString();
        //            return false;
        //        }
        //    }

        //    //Bind SSL Certificate to Port
        //    string BindCommand = "netsh http add sslcert ipport=0.0.0.0:" + PortNumber + " certhash=" + SSLCertificate.Thumbprint + " appid={" + applicationId + "}";
        //    Logger.Log("Binding newly generated SSL Certificate to Port " + PortNumber + " via Command=" + BindCommand);
        //    try
        //    {
        //        string BindResultText = ExecuteCommand(BindCommand).RemoveAllNewLines().Trim(' ');
        //        Logger.Log(BindResultText);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExStr = "Unable to bind generate SSL Certificate to Port " + PortNumber + "\r\n" + ex.ToString();
        //        return false;
        //    }
        //    return true;
        //}



        //public static X509Certificate2 FindExistingCert_CreateCAIfNotExist(string SSLCertName, string[] SubjectAlternativeNames_ForHttpSSLCert, 
        //    out AsymmetricKeyParameter CAPrivateKey, Action<string> )
        //{
        //    X509Certificate2 SSLCertificate = null;

        //    //Check if Root CA exists in Store
        //    CAPrivateKey = null;
        //    Logger.Log("Checking if CA(Certificate Authority) Cert '" + SSLCertName + " CA' exists in 'Trusted Root Certificate Authotirities > Certificates' Store...");
        //    X509Store storeCA = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        //    storeCA.Open(OpenFlags.ReadWrite);
        //    X509Certificate2 CACertificate = null;
        //    foreach (X509Certificate2 CertCA in storeCA.Certificates)
        //    {
        //        try
        //        {
        //            if (CertCA.FriendlyName == SSLCertName + " CA" && CertCA.NotBefore < DateTime.Now.AddDays(-7) && CertCA.PrivateKey != null)
        //            {
        //                CACertificate = CertCA;
        //                Logger.Log("Found CA(Certificate Authority) Cert '" + SSLCertName + " CA' in 'Trusted Root Certificate Authotirities > Certificates'");
        //                RSACryptoServiceProvider key = (RSACryptoServiceProvider)CACertificate.PrivateKey;
        //                RSAParameters rsaparam = key.ExportParameters(true);
        //                AsymmetricCipherKeyPair keypair = DotNetUtilities.GetRsaKeyPair(rsaparam);
        //                CAPrivateKey = keypair.Private;
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Log("ERROR: trying to read Certificate from Cert Store '" + CertCA.ToString() + "'\r\n" + ex.ToString());
        //        }
        //    }


        //    //Generate Root CA if Root CA does not Exist in Store
        //    if (CACertificate == null)
        //    {
        //        Logger.Log("CA (Certificate Authority) Cert '" + SSLCertName + " CA' not found. Creating CA Certificate...");
        //        //X509Certificate2 certificateAuthorityCertificate = CreateCertificateAuthorityCertificate("CN=" + CertSubjectName + " CA", out CAPrivateKey);
        //        X509Certificate2 certificateAuthorityCertificate = CreateCertificateAuthorityCertificate(SSLCertName, out CAPrivateKey);

        //        Logger.Log("Adding CA Cert to 'Trusted Root Certificate Authority' Store in LocalMachine...");
        //        X509Store StoreToAddCACert = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        //        StoreToAddCACert.Open(OpenFlags.ReadWrite);
        //        StoreToAddCACert.Add(certificateAuthorityCertificate); // A Cetificate with Issued To = 'Vlix CA' should not exist in certmgr
        //        StoreToAddCACert.Close();
        //    }

        //    //Check if SSL Certificate exists in Store
        //    Logger.Log("Checking if SSLCert '" + SSLCertName + "' exists in Certificate Store...");
        //    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        //    store.Open(OpenFlags.ReadWrite);
        //    SSLCertificate = null;
        //    List<X509Certificate2> SSLCertificatesToDelete = new List<X509Certificate2>();
        //    foreach (X509Certificate2 Cert in store.Certificates)
        //    {
        //        //if (Cert.FriendlyName == "Vlix")
        //        //if (Cert.FriendlyName == SSLCertName)
        //        if (Cert.FriendlyName == SSLCertName && Cert.Issuer == "CN=Vlix CA")
        //        {
        //            if (Cert.NotBefore < DateTime.Now.AddDays(-7))
        //            {
        //                if (SSLCertificate == null)
        //                {
        //                    //Check if Subject Altenative Names are the same
        //                    bool SubjectAlternativeNamesAreSame = false;
        //                    foreach (System.Security.Cryptography.X509Certificates.X509Extension Extension in Cert.Extensions)
        //                    {
        //                        if (Extension.Oid.FriendlyName == "Subject Alternative Name")
        //                        {
        //                            AsnEncodedData asndata = new AsnEncodedData(Extension.Oid, Extension.RawData);
        //                            string[] SANS = (asndata.Format(true)).Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        //                            List<string> SubjectAlternativeNames_Cur = new List<string>();
        //                            List<string> SubjectAlternativeNames_New = SubjectAlternativeNames_ForHttpSSLCert.ToList();
        //                            foreach (string SANEntry in SANS)
        //                            {
        //                                string SAN = SANEntry.Split('=')[1];
        //                                SubjectAlternativeNames_Cur.Add(SAN);
        //                            }
        //                            List<string> SubjectAlternativeNamesNotInCurList = SubjectAlternativeNames_Cur.Except(SubjectAlternativeNames_New).ToList();
        //                            List<string> SubjectAlternativeNamesNotInNewList = SubjectAlternativeNames_New.Except(SubjectAlternativeNames_Cur).ToList();
        //                            SubjectAlternativeNamesAreSame = (SubjectAlternativeNamesNotInCurList.Count == 0 && SubjectAlternativeNamesNotInNewList.Count == 0);
        //                            break;
        //                        }
        //                    }
        //                    if (SubjectAlternativeNamesAreSame)
        //                    {
        //                        Logger.Log("Found SSLCert '" + SSLCertName + "' in Certificate Store...");
        //                        SSLCertificate = Cert;
        //                    }
        //                    else SSLCertificatesToDelete.Add(Cert);
        //                }
        //                else SSLCertificatesToDelete.Add(Cert);
        //            }
        //            else SSLCertificatesToDelete.Add(Cert);
        //        }
        //    }
        //    foreach (X509Certificate2 CertToDelete in SSLCertificatesToDelete) store.Remove(CertToDelete);
        //    store.Close();
        //    if (SSLCertificate == null) Logger.Log("SSLCert '" + SSLCertName + "' was not found in Certificate Store...");
        //    return SSLCertificate;
        //}



        //public static X509Certificate2 CreateCertificateAuthorityCertificate(string SSLCertName, out AsymmetricKeyParameter CaPrivateKey)
        //{
        //    X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

        //    //Generate Assymmetric Key Pair
        //    CaPrivateKey = null;
        //    const int keyStrength = 2048;
        //    CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
        //    SecureRandom random = new SecureRandom(randomGenerator);
        //    RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
        //    keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(randomGenerator), keyStrength));
        //    AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        //    // Set Details
        //    BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
        //    certificateGenerator.SetSerialNumber(serialNumber);
        //    X509Name subjectDN = new X509Name("CN=" + SSLCertName + " CA");
        //    certificateGenerator.SetIssuerDN(subjectDN);
        //    certificateGenerator.SetSubjectDN(subjectDN);
        //    DateTime notBefore = DateTime.Now.AddYears(-1);
        //    certificateGenerator.SetNotBefore(notBefore);
        //    certificateGenerator.SetNotAfter(notBefore.AddYears(8));
        //    certificateGenerator.SetPublicKey(subjectKeyPair.Public);


        //    // Generating the CA Certificate
        //    Org.BouncyCastle.X509.X509Certificate BCCACertificate = certificateGenerator.Generate(new Asn1SignatureFactory("SHA512WITHRSA", subjectKeyPair.Private, random));

        //    //X509Certificate2 x509 = new X509Certificate2(BCCACertificate.GetEncoded(), "password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        //    //x509.FriendlyName = "Vlix CA";
        //    //CaPrivateKey = subjectKeyPair.Private;
        //    //return x509;


        //    X509Certificate2 x509 = new X509Certificate2(BCCACertificate.GetEncoded(), "password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        //    PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);
        //    Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
        //    RsaPrivateKeyStructure rsa = RsaPrivateKeyStructure.GetInstance(seq); //new RsaPrivateKeyStructure(seq);
        //    RsaPrivateCrtKeyParameters rsaparams = new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);
        //    //x509.FriendlyName = "Vlix CA";
        //    x509.FriendlyName = SSLCertName + " CA";
        //    x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
        //    byte[] certBytes = x509.Export(X509ContentType.Pkcs12, "Vlix");
        //    X509Certificate2 CACertificate = new X509Certificate2(certBytes, "Vlix", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        //    X509Store Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        //    Store.Open(OpenFlags.MaxAllowed);
        //    Store.Add(CACertificate);
        //    Store.Close();
        //    CaPrivateKey = subjectKeyPair.Private;
        //    return x509;
        //}


        public static string ExecuteCommand(string action)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Normal, FileName = "cmd.exe", UseShellExecute = false, RedirectStandardOutput = true, Arguments = "/c " + action }
            })
            {
                process.Start();
                while (!process.StandardOutput.EndOfStream) stringBuilder.AppendLine(process.StandardOutput.ReadLine());
                process.Close();
            }
            return stringBuilder.ToString();
        }
        //TODO
        //Import Nuget Package CERTES
        // https://github.com/fszlin/certes
        //public async static Task<X509Certificate2> GetLetsEncryptCertificate(string email, string certificateName)
        //{
        //    var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2);
        //    var account = await acme.NewAccount(email, true);


        //    var order = await acme.NewOrder(new[] { certificateName });
        //    var authz = (await order.Authorizations()).First();
        //    var httpChallenge = await authz.Http();
        //    var keyAuthz = httpChallenge.KeyAuthz;

        //    return new X509Certificate2();
        //}
    }
}