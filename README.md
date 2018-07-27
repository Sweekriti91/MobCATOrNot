# MobCATOrNot

This is an app demonstrating how to use CoreML and TensorFlow with Xamarin to build an image detection app. This app also uses the [Xamarin.Forms Shell Spec](https://github.com/davidortinau/Gastropods). 

The model it uses was trained with [Microsoft's Custom Vision service](https://www.customvision.ai).


## Building the Project
All analysis is done using Custom Vision services. To use this solution, you must setup a model and then fill in the API Keys inside of [**Constants.cs**](MobCATOrNot/Constants.cs)

## Custom Vision Services
- All the interesting code is in [CustomVisionAPIService.cs](MobCATOrNot/Services/CustomVisionAPIService.cs). You can find the interfaces where we have wrapped the Custom Vision API. 

- For the Vision Models, the interface used by Xamarin.Forms can be found in [ICustomVisionService.cs](MobCATOrNot/Services/ICustomVisionService.cs). Each platform level implementation can be found in their respective folders. For iOS, which uses CoreML can be found [here](MobCATOrNot.iOS/Services/CustomVisionService.cs) and for Android, which uses TensorFlow can be found [here](MobCATOrNot.Android/Services/CustomVisionService.cs)

## Xamarin Forms
The app has been written using Xamarin.Forms and the Early Preview of the [Shell Container Spec](https://github.com/davidortinau/Gastropods). 

## Contributors

* [Alexey Strakh](https://github.com/alexeystrakh)
* [Sweekriti Satpathy](https://github.com/Sweekriti91)
