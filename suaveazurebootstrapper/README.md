# suave.io on Azure

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

This repository shows how to use [FAKE](https://github.com/fsharp/FAKE), [Paket](https://github.com/fsprojects/Paket) and [KuduSync](https://github.com/projectkudu/KuduSync) to deploy a [suave.io](http://suave.io/) website to Azure.

## Getting started

* Go to http://azure.microsoft.com/ and create an Azure account
* Clone this repo and follow the setup steps from https://azuredeploy.net/
* Congratulations your first suave.io website is running on Azure!
* (Optional) Look at your Azure management portal and copy the deployment trigger url to the webhooks of your github repo.
   * This allows you to trigger deployments via `git push origin master`.   

## How is it working?

Whenever you push to Azure the `deploy.cmd` will be run and 

  * it downloads the latest paket.exe and uses it to restore the NuGet packages from the `paket.dependencies`
  * it uses FAKE to execute the `build.fsx` which can be used to compile an application  
  * it uses [KuduSync](https://github.com/projectkudu/KuduSync) to synchronize the changes to your Website
  * it uses FAKE to run the `site/webserver.fsx` script which then starts a [suave.io](https://github.com/SuaveIO/suave) webserver

## Going further
      
This is a basic setup and only starts a very small FAKE script on Azure. 
Feel free to modify the build and webserver script or you might even want to start a different application from it.
If you need more NuGet packages then modify the `paket.dependencies` file and run `.paket/paket.exe install`. 
You can find more information in the [Paket docs](http://fsprojects.github.io/Paket/).
