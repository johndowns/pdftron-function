# Use PDFTron from an Azure Function

This sample demonstrates how to use an Azure Function (written in C#/.NET Core 3.1) with the [PDFTron](https://www.pdftron.com/) SDK, to generate PDF files.

## Sample scenarios

The sample Azure Functions app has two functions, each corresponding to a documented PDFTron sample:
 * `AddImageTest` is based on the [`AddImage` sample](https://www.pdftron.com/documentation/samples/dotnetcore/cs/AddImageTest?platforms=dotnetcore). This illustrates how to embed images into PDF files.
 * `ElementBuilderTest` is based on the [`ElementBuilder` sample](https://www.pdftron.com/documentation/samples/dotnetcore/cs/ElementBuilderTest?platforms=dotnetcore). This illustrates how to use custom fonts, images, and other advanced PDF features.

You should test your own specific scenarios to ensure they work correctly within the Azure Functions sandbox.

## Function app 
This sample requires that you use one of the following Azure Functions hosting options:
 * The Elastic Premium plan.
 * Any of the dedicated App Service plan options, from the Basic tier above.

The sample will not work on the Azure Functions consumption plan due to sandbox limitations.

Furthermore, you need to ensure you configure your function app to run in 64-bit mode so that it can load and run the PDFTron binaries.

The [ARM template](deploy/template.json) in this sample includes these configuration steps.

## Running this sample

To run this yourself:
 * Deploy the app by running the `deploy/deploy.ps1` script. You will need to have the [v3 Azure Functions CLI installed](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#v2) to successfully deploy the function.
 * Use the URLs emitted by the deployment script to test the function. You can open these in a browser to see the generated PDF files.

Please note that PDFTron is a commercial product and needs a licence for production use.
