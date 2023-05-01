#Requires -Version "7.0"
#Requires -RunAsAdministrator

# required to start WCF hosts from example\*WCF* without elevated permissions
$user = whoami

netsh http add urlacl url=http://+:8000/DebugService.svc user=$user
netsh http add urlacl url=http://+:8000/PersonService.svc user=$user