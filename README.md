
<div align="center">
 <img align="center" src="https://procesio-email-hosting.fra1.digitaloceanspaces.com/logo-procesio.png" />
  <h2>Procesio SDK dotnet</h2>
  <blockquote>Minimal library for running Procesio processes</blockquote>

<strong>For a typescript alternative, check out [procesio-sdk-typescript](https://github.com/PROCESIO/procesio-sdk-typescript.git).</strong>

</div>

## 📦 Installation

There are multiple ways to install ProcesioSDK.

### Package Manager Console

 - in Visual Studio use the "Tools" -> "NuGet Package Manager" -> "Package Manager Console" menu
 - run the following command:
 ```
 PM> Install-Package ProcesioSDK
 ```

### Manage NuGet Packages for Solution

 - in Visual Studio use the "Tools" -> "NuGet Package Manager" -> "Manage NuGet Packages for Solution" menu
 - go to "Browse" tab
 - search for **ProcesioSDK** and select it
 - in the ProcesioSDK pane select your project and press the "Install" button

### Build from repo

 Build **ProcesioSDK** from this repository and add it to your project.

## Usage

 First you need to import the appropriate resources:
 ```
 using ProcesioSDK;
 using ProcesioSDK.Config;
 using ProcesioSDK.Dto.Data;
 ```
 Then you will need to create an instance of the ProcesioClient. You have 2 options, either you have ProcesioConfig section within appsettings file, or you use the ProcesioConfig object to access the Hosted Procesio server.
 
 ```
 var config = new ProcesioConfig()
        {
            ServerUri = " ",
            AuthenticationUri = " ",
            AuthenticationRealm = " ",
            AuthenticationClientId = " "
        };
 ProcesioClient client = new ProcesioClient(config);
 ```

 After that you will need to authenticate. The Authentication requires a ProcesioUser object, which contains Procesio credentials, as username, password. The Authentication will return a ProcesioToken object (access token, refresh token and token valability).
 
 ```
 ProcesioUser procesioUser = new ProcesioUser("username", "password");
 ProcesioToken procesioTokens = await client.Authenticate(procesioUser);
 ```
 
 Running a process from PROCESIO is done in 3 different steps.
 ### 1. Publish a process
 
 The first step requires you to publish that process with all the needed inputs, defined when you created the process. 
 To call the PublishProcess method, you will need 4 parameters:
 1. the process id to execute; 
 2. the input values used by the process, a dictionary that contains the name of the variables used for process and their values; 
 3. the ProcesioTokens object, returned by Authenticate method; 
 4. the workspace name, which can be null if you are working on the personal workspace.
 
 This call will return a ProcessInstance object from which you require 2 infos: instanceID (Id) and the variable list (Variables). Instance id is needed for both uploading a file and for launching the process. Variable list is needed for uploading the files.
	
 ### 2. Upload file  
 
 The second step, upload file, is optional and only needed for processes that have file inputs. 
 To run this step you will need:
 1. the instanceID;
 2. the file details, as file path, variable name, lenght;
 3. the ProcesioTokens object, returned by Authenticate method; 
 4. the workspace name, which can be null if you are working on the personal workspace.
 
 ### 3. Launch a process
 
 The third step, launches the process should be run only after the files have been uploaded.
 To call the LaunchProcessInstance method, you will need 3 parameters:
 1. the instanceID;
 2. the ProcesioTokens object, returned by Authenticate method; 
 3. the workspace name, which can be null if you are working on the personal workspace.
 
 There is the possibility to synchronously launch the process. And, in addition to the above parameters, a timeout is required.
    
 ### Run a process (Publish + Launch)
	
 The RunProcess method is the alternative to Publish and Launch a process.
 To call this you will need 5 parameters:
 1. the process id to execute; 
 2. the input values  used by the process, a dictionary that contains the name of the variables used for process and their values;
 3. the ProcesioTokens object, returned by Authenticate method; 
 4. the workspace name, which can be null if you are working on the personal workspace;
 5. the input file(s) (the variable name, the file content and the file name with extension), which can be null if no files are required.
 
 There is the possibility to synchronously run the process. And, in addition to the above parameters, a timeout is required.
	
 ## Example: Execute a process which sends an email (without attachments)
	
	```
	// Publish the process
	var flow = await client.PublishProcess("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx", {to: "someemail@domain.com", subject: "Process launched via SDK"}, procesioTokens, "workspaceName");
	var processInstance = flow.Content; // Get the content returned by PublishProcess method
	```
	
	```
	// Launch the process
	var idLaunch = await client.LaunchProcessInstance(processInstance.Id.ToString(), procesioTokens, "workspaceName");
	```
	
	
	```
	// Run the process
	var idRun = await client.RunProcess("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx", {to: "someemail@domain.com", subject: "Process launched via SDK"}, procesioTokens, "workspaceName", null);
	```
