# {{ name }}
## {{ integration_type | capitalize }}

{{ description }}

# Prerequisites

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the Command Server certificate store

# Install
* Download latest successful build from GitHub :<br/>
[GitHub Releases](https://github.com/Keyfactor/sectigo-certmanager-cagateway/releases)

* Copy SectigoCAProxy.dll to the Program Files\Keyfactor\ Keyfactor AnyGateway directory

* Update the CAProxyServer.config file
  * Update the CAConnection section to point at the SectigoCAProxyclass
  ```xml
  <alias alias="CAConnector" type="Keyfactor.AnyGateway.Sectigo.SectigoCAProxy, SectigoCAProxy"/>
  ```

# Configuration
The following sections will breakdown the required configurations for the AnyGatewayConfig.json file that will be imported to configure the AnyGateway.

## Templates
The Template section will map the CA's SSL profile to an AD template. Currently the only required parameter is the MultiDomain flag. This flag lets Keyfactor know if the certificate can contain multiple domain names.  Depending on the setting, the SAN entries of the request will change to support Sectigo Requirements. 
 ```json
  "Templates": {
	"SectigoEnterpriseSSLPro1yr": {
           "ProductID": "3210", /*Sectigo EnterpriseSSL Pro - ID from Cert Manager*/
           "Parameters": {
                 "MultiDomain": "false"
      }
   }
}
 ```
## Security
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
## CerificateManagers
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
## CAConnection
The CA Connection section will determine the API endpoint and configuration data used to connect to Sectigo Cert Manager. 
* ```ApiEndpoint```
This is the endpoint used by the Gateway to connect to the API. There are a few possible values depending on the Customer's configuration.  
* ```AuthType```
This value must be Password or Certificate.  It will determine what credentials are used to connect to the API
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
This is the number of certificates per request that will be processed during sync. 
*```ExternalRequestorFieldName```
This is the Enrollment Field name that can be populated to pass an email address to Sectigo for enrollment notifications.  If blank, the API will default to the email address of the API user configured in the username field above. 
*```SyncFilter```
This object will allow the implementation team to determine how the synchronization process limits certificates.  All SSL List filter parameters should be supported.  The example below shows filtering based on specific templates that should only be synchronized by a particular CA.[Support Article for API detail](https://support.sectigo.com/Com_KnowledgeDetailPage?Id=kA01N000000XDkE)

```json
  "CAConnection": {
	"ApiEndpoint":"https://hard.cert-manager.com/",
	"AuthType":"Password",
	"Username":"Username",
	"Password":"ThisIsMyPassword",
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
## GatewayRegistration
There are no specific Changes for the GatewayRegistration section. Refer to the Refer to the AnyGateway Documentation for more detail.
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

## ServiceSettings
There are no specific Changes for the GatewayRegistration section. Refer to the Refer to the AnyGateway Documentation for more detail.
```json
  "ServiceSettings": {
    "ViewIdleMinutes": 8,
    "FullScanPeriodHours": 24,
	"PartialScanPeriodMinutes": 480 /*Note partial sync based on a timestamp is not supported by the Sectigo API. As a result all syncs with the API are treated as full syncronization jobs*/
  }
```
