
# Sectigo Certificate Manager

Sectigo Certificate Manager is a private certificate authority designed for enterprise use. The Sectigo Gateway enables the following certificate authority management functions via Keyfactor Command: Enrollment of new certificates, Revocation of existing certificates, and Synchronization of previously issued certificates.

#### Integration status: Production - Ready for use in production environments.

## About the Keyfactor AnyCA Gateway DCOM Connector

This repository contains an AnyCA Gateway Connector, which is a plugin to the Keyfactor AnyGateway. AnyCA Gateway Connectors allow Keyfactor Command to be used for inventory, issuance, and revocation of certificates from a third-party certificate authority.

## Support for Sectigo Certificate Manager

Sectigo Certificate Manager is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com

###### To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

---


---





## Keyfactor AnyCA Gateway Framework Supported
The Keyfactor gateway framework implements common logic shared across various gateway implementations and handles communication with Keyfactor Command. The gateway framework hosts gateway implementations or plugins that understand how to communicate with specific CAs. This allows you to integrate your third-party CAs with Keyfactor Command such that they behave in a manner similar to the CAs natively supported by Keyfactor Command.




This gateway extension was compiled against version  of the AnyCA Gateway DCOM Framework.  You will need at least this version of the framework Installed. If you have a later AnyGateway Framework Installed you will probably need to add binding redirects in the CAProxyServer.exe.config file to make things work properly.


[Keyfactor CAGateway Install Guide](https://software.keyfactor.com/Guides/AnyGateway_Generic/Content/AnyGateway/Introduction.htm)



---


# Sectigo Certificate Manager
## Ca-gateway

Sectigo Certificate Manager is a private certificate authority designed for enterprise use. The Sectigo Gateway enables the following certificate authority management functions via Keyfactor Command: Enrollment of new certificates, Revocation of existing certificates, and Synchronization of previously issued certificates.

Additional information about [Sectigo Certificate Manager](https://sectigo.com/knowledge-base/product/Sectigo_Certificate_Manager_SCM)

### Supported Functionality
* SSL Certificate Synchronization
    * Sync can be filtered by any available SSL Certificate List filter defined by the Cert Manager API
    * All Sync jobs are treated as a full sync becuase the Cert Manager API does not allow for filtering based on a date/time stamp
    * Certificates will only syncronize once.  If a certificate is found based on Serial Number for the managed CA it will be skipped for subsequent syncs to minimize impact on Cert Manager API load

* SSL Certificate Enrollment
   * Note about organizations.  The organization for enrollment is selected based on the Organization subject field, as well as any Department specified in the template configuration. If a department is specified, and that department exists within the organization and is valid for issuing certs, the department ID will be used. If no department is specified, the organization ID will be used if the organization is valid for issuing certs. If the organization/department are not valid for issuing certs, the enrollment will fail, as that is a required field for Sectigo.
* SSL Certificate Revocation

### Not Implemented/Supported
* Device Certificates
* Client Certificates
* Code Signing

## Compatibility
This AnyGateway is designed to be used with version 21.3.2 or newer of the Keyfactor AnyGateway DCOM Framework

## Prerequisites

### Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the Command Server certificate store

### Sectigo Settings
For each Organization/Department you plan on using through the gateway, in your Sectigo portal, go to that Organization, select Certificate Settings -> SSL Certificates, and check the "Enable Web/REST API" checkbox.  
In addition, for the admin account you plan to use, make sure it has the API admin type selected in the portal.

### Migration
In the event that a system is being upgraded from the Legacy Sectigo CA Gateway (19.4 or older), a migration from the legacy database format to the AnyGateway format will be required. 

To begin the migration process, the DatabaseManagementConsole.exe.config will need to be updated to reference the SectigoEsentMigrator.  This is one by modifying the mapping for the IDatabaseMigrator inteface in the config file. 
```xml
<register type="IDatabaseMigrator" mapTo="Keyfactor.AnyGateway.Sectigo.Database.SectigoEsentMigrator, SectigoEsentMigrator" />
```

## Install
* Download latest successful build from GitHub :<br/>
[GitHub Releases](https://github.com/Keyfactor/sectigo-certmanager-cagateway/releases)

* Copy SectigoCAProxy.dll to the Program Files\Keyfactor\ Keyfactor AnyGateway directory

* Update the CAProxyServer.config file
  * Update the CAConnection section to point at the SectigoCAProxyclass
  ```xml
  <alias alias="CAConnector" type="Keyfactor.AnyGateway.Sectigo.SectigoCAProxy, SectigoCAProxy"/>
  ```

## Configuration
The following sections will breakdown the required configurations for the AnyGatewayConfig.json file that will be imported to configure the AnyGateway.

### Templates
The Template section will map the CA's SSL profile to an AD template. The following parameters are accepted:
* ```MultiDomain```
REQUIRED. This flag lets Keyfactor know if the certificate can contain multiple domain names.  Depending on the setting, the SAN entries of the request will change to support Sectigo Requirements. 
* ```Organization```
OPTIONAL. If the organization name is provided as a parameter here, the Sectigo gateway will use that organization name in requests, instead of the O field in the subject.
* ```Department```
OPTIONAL. If your Sectigo account is using department-level products, put the appropriate department name here. Previous versions of the Sectigo gateway read this value from the OU field of the subject, which is now deprecated.

 ```json
  "Templates": {
	"SectigoEnterpriseSSLPro1yr": {
           "ProductID": "3210", /*Sectigo EnterpriseSSL Pro - ID from Cert Manager*/
           "Parameters": {
                 "MultiDomain": "false",
				 "Organization": "Organization Name",
				 "Department": "Department Name"
      }
   }
}
 ```
### Security
The security section does not change specifically for Sectigo Cert Manager.  Refer to the AnyGateway Documentation for more detail.
```json
  /*Grant permissions on the CA to users or groups in the local domain.
	READ: Enumerate and read contents of certificates.
	ENROLL: Request certificates from the CA.
	OFFICER: Perform certificate functions such as issuance and revocation. This is equivalent to "Issue and Manage" permission on the Microsoft CA.
	ADMINISTRATOR: Configure/reconfigure the gateway.
	Valid permission settings are "Allow", "None", and "Deny".*/
    "Security": {
        "Keyfactor\\Administrator": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        },
        "Keyfactor\\gateway_test": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        },		
        "Keyfactor\\SVC_TimerService": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "None"
        },
        "Keyfactor\\SVC_AppPool": {
            "READ": "Allow",
            "ENROLL": "Allow",
            "OFFICER": "Allow",
            "ADMINISTRATOR": "Allow"
        }
    }
```
### CerificateManagers
The Certificate Managers section is optional.
	If configured, all users or groups granted OFFICER permissions under the Security section
	must be configured for at least one Template and one Requester. 
	Uses "<All>" to specify all templates. Uses "Everyone" to specify all requesters.
	Valid permission values are "Allow" and "Deny".
```json
  "CertificateManagers":{
		"DOMAIN\\Username":{
			"Templates":{
				"MyTemplateShortName":{
					"Requesters":{
						"Everyone":"Allow",
						"DOMAIN\\Groupname":"Deny"
					}
				},
				"<All>":{
					"Requesters":{
						"Everyone":"Allow"
					}
				}
			}
		}
	}
```
### CAConnection
The CA Connection section will determine the API endpoint and configuration data used to connect to Sectigo Cert Manager. 
* ```ApiEndpoint```
This is the endpoint used by the Gateway to connect to the API. There are a few possible values depending on the Customer's configuration. NOTE: If doing mTLS/Certificate Auth, this endpoint should end in /private/ (see examples below)
* ```AuthType```
This value must be Password or Certificate.  It will determine what credentials are used to connect to the API. NOTE: mTLS/Certificate Auth will not work properly if there is a proxy doing TLS inspection.
* ```Username```  
This is the username associated with the API login and will determine the security role in the Certificate Manager platform. 
* ```Password```
If AuthType is Password, this is the password assoicated with the API login. Otherwise it is ignored.
* ```CustomerUri```
This is a static value that determined the customer's account on the Certificate Platform.  This can be found as part of the portal login URL https://hard.cert-manager.com/customer/{CustomerUri}
* ```ClientCertificate```
If AuthType is Certificate, this section will determine which certificate is used by the Gateway to authenticate to the API.  It will search the Windows Certficaite Store by Location and Thumbprint to find the correct certificate.  The network service account must have access to the Certificate and Key Material for certificate authentication to work. 
* ```PickupRetries```
This setting determine the number of times the service will attempt to download a certificate after successful enrollment. If the certificate cannot be downloaded during this retry period it will be picked up during the next sync. 
* ```PickupDelay```
This is the number of seconds between retries.  Be aware that the total # of retries times the number of seconds will be an amount of time the portal will be occupied during enrollment. If the duration is too long the request may timeout and cause unexpected results. 
* ```PageSize```
This is the number of certificates per request that will be processed during sync. If not provided, default is 25.
* ```ExternalRequestorFieldName```
This is the Enrollment Field name that can be populated to pass an email address to Sectigo for enrollment notifications.  If blank, the API will default to the email address of the API user configured in the username field above. 
* ```SyncFilter```
This object will allow the implementation team to determine how the synchronization process limits certificates. If not provided, no filtering will be done, and all certs will be returned. All SSL List filter parameters should be supported.  The example below shows filtering based on specific templates that should only be synchronized by a particular CA.[Support Article for API detail](https://support.sectigo.com/Com_KnowledgeDetailPage?Id=kA01N000000XDkE)

Password AuthType:
```json
  "CAConnection": {
	"ApiEndpoint":"https://hard.cert-manager.com/",
	"AuthType":"Password",
	"Username":"Username",
	"Password":"ThisIsMyPassword",
	"CustomerUri":"findmeintheportal-url",
	"PickupRetries":5,
	"PickupDelay":10,
	"PageSize":100,
	"ExternalRequestorFieldName":"",
	"SyncFilter":{
		"sslTypeId":["14078","14079","14065"]
	}
  }
```

Certificate AuthType:
```json
  "CAConnection": {
	"ApiEndpoint":"https://hard.cert-manager.com/private/",
	"AuthType":"Certificate",
	"Username":"Username",
	"CustomerUri":"findmeintheportal-url",
	"ClientCertificate":{
		"StoreName": "My",
		"StoreLocation": "LocalMachine",
		"Thumbprint": "5be9415658b26f3d0805f8ccfc1e9e2450b90450"
	},
	"PickupRetries":5,
	"PickupDelay":10,
	"PageSize":100,
	"ExternalRequestorFieldName":"",
	"SyncFilter":{
		"sslTypeId":["14078","14079","14065"]
	}
  }
```

### GatewayRegistration
There are no specific Changes for the GatewayRegistration section. Refer to the AnyGateway Documentation for more detail.
```json
  "GatewayRegistration": {
    "LogicalName": "SectigoCASandbox",
    "GatewayCertificate": {
      "StoreName": "CA",
      "StoreLocation": "LocalMachine",
      "Thumbprint": "bc6d6b168ce5c08a690c15e03be596bbaa095ebf"
    }
  }
```

### ServiceSettings
There are no specific Changes for the GatewayRegistration section. Refer to the AnyGateway Documentation for more detail.
```json
  "ServiceSettings": {
    "ViewIdleMinutes": 8,
    "FullScanPeriodHours": 24,
	"PartialScanPeriodMinutes": 480 /*Note partial sync based on a timestamp is not supported by the Sectigo API. As a result all syncs with the API are treated as full syncronization jobs*/
  }
```


