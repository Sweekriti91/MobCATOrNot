# MobCATOrNot

This is an app demonstrating how to use CoreML and TensorFlow with Xamarin to build an image detection app. This app also uses the [Xamarin.Forms Shell](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/shell/create). 

The model it uses was trained with [Microsoft's Custom Vision service](https://www.customvision.ai).


## PreRequisite

- Create and deploy [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/) Project and add the url and keys to [**Constants.cs**](MobCATOrNot/Constants.cs)


- All analysis is done using Custom Vision services. To use this solution, you must setup a model and then fill in the API Keys inside of [**Constants.cs**](MobCATOrNot/Constants.cs)

## Custom Vision Services
- All the interesting code is in [CustomVisionAPIService.cs](MobCATOrNot/Services/CustomVisionAPIService.cs). You can find the interfaces where we have wrapped the Custom Vision API. 

- For the Vision Models, the interface used by Xamarin.Forms can be found in [ICustomVisionService.cs](MobCATOrNot/Services/ICustomVisionService.cs). Each platform level implementation can be found in their respective folders. For iOS, which uses CoreML can be found [here](MobCATOrNot.iOS/Services/CustomVisionService.cs) and for Android, which uses TensorFlow can be found [here](MobCATOrNot.Android/Services/CustomVisionService.cs)


## Contributors

* [Alexey Strakh](https://github.com/alexeystrakh)
* [Sweekriti Satpathy](https://github.com/Sweekriti91)
