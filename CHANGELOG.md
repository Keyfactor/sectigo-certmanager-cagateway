# 1.0.0
* Initial Release with support for SSL Certificate Synchronization, Revocation, and Enrollment (New, Renew, Reissue)
# 1.0.1
* Enhance the handling of exceptions during enrollment.  Organization and Org Unit checks will now return a failed enrollment result vs. thowing an exception. 
# 1.0.2
* Minor bug fix
# 1.0.3
* Documentation updates, EsentMigration, and Update Package References
# 1.0.4
* Workflow updates, documentation, and enhanced EV support
# 1.0.7
* OU deprecation, added Parameters field to the Template section in the config to specify department name, if needed.
# 1.0.9
* Made Department field optional
# 1.0.10
* Handle change to Sectigo API for organization/department lookups
# 1.1.0
* Allow organization name to be provided in the template section of the config
# 1.2.0
* Allow for blank CN to be provided
* Fixes for Certificate Authentication  
# 1.2.1  
* Fix for handling sync of expired records  
* Handle null Keyfactor-Requestor  
* Properly pass in custom fields to enrollment API  
# 1.2.2  
* Change revocation API call to match new Sectigo request format  
# 1.2.3  
* Fix for JSON serialization for revocation  