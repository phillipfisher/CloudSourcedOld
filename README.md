# CloudSourced
CloudSourced is a Cloud Based Solution for Event Sourcing using Microsoft's Service Fabric.
## Why this project was created?
We were looking for a Event Sourcing solution that took advantage of the Azure cloud's PAAS and SAAS services.  There were several very nice solutions that on top of Linux VMs but we wanted something Infracsture-less.  Service Fabric itself can be ran on any cloud or on prem environment, but is native to Azure and easy to deploy.
## How does it work?
The application runs within Service Fabric and every stream is itself a seperate Actor and as such acts independently from other streams. Each message is sent to it and versioned so that multiple different systems can read/write simultaneously if desired.
## Where is the data stored?
The data is persisted in Blob storage on Azure using Append Blobs.
