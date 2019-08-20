using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceDetection
{
    class Program
    {
        private const string subscriptionKey = "097d8aad07f64648a59969e36e4d4c6f";

        private const string faceEndpoint =
            "https://westcentralus.api.cognitive.microsoft.com";

        // localImagePath = @"C:\Documents\LocalImage.jpg"
        private const string localImagePath = @"<LocalImage>";

        private const string remoteImageUrl =
            "https://ak3.picdn.net/shutterstock/videos/24915083/thumb/5.jpg";

        private static readonly FaceAttributeType[] faceAttributes =
            { FaceAttributeType.Age, FaceAttributeType.Gender };

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = faceEndpoint;

            Console.WriteLine("Faces being detected ...");
            var t1 = DetectRemoteAsync(faceClient, remoteImageUrl);
            //var t2 = DetectLocalAsync(faceClient, localImagePath);

            Task.WhenAll(t1).Wait(5000);
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static async Task DetectRemoteAsync(
            FaceClient faceClient, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
                return;
            }

            try
            {
                IList<DetectedFace> faceList =
                    await faceClient.Face.DetectWithUrlAsync(
                        imageUrl, true, true, faceAttributes);

                DisplayAttributes(GetFaceAttributes(faceList, imageUrl), imageUrl);
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(imageUrl + ": " + e.Message);
            }
        }

        private static string GetFaceAttributes(IList<DetectedFace> faceList, string imagePath)
        {
            string attributes = string.Empty;

            foreach (DetectedFace face in faceList)
            {
                if (face.FaceLandmarks != null)
                {
                    int eyeLeftYBottom = Convert.ToInt32(Math.Truncate(face.FaceLandmarks.EyeLeftBottom.Y)) + 1;
                    int eyeLeftYTop = Convert.ToInt32(Math.Truncate(face.FaceLandmarks.EyeLeftTop.Y)) - 1;
                    int eyeLeftXInner = Convert.ToInt32(Math.Truncate(face.FaceLandmarks.EyebrowLeftInner.X)) + 1;
                    int eyeLeftXOuter = Convert.ToInt32(Math.Truncate(face.FaceLandmarks.EyebrowLeftOuter.X)) - 1;

                    int leftEyeDiffHeight = eyeLeftYBottom - eyeLeftYTop;
                    int leftEyeDiffWidth = eyeLeftXInner - eyeLeftXOuter;

                    uint[,] newImage = new uint[leftEyeDiffHeight, leftEyeDiffWidth];

                    var origImage = LoadImage("5.jpg");

                    for (int row = eyeLeftYTop; row < eyeLeftYBottom; row++)
                    {
                        for (int col = eyeLeftXOuter; col < leftEyeDiffHeight; col++)
                        {
                        }
                    }
                }

                //double? age = face.FaceAttributes.Age;
                //string gender = face.FaceAttributes.Gender.ToString();
                //attributes += gender + " " + age + "   ";
            }

            return attributes;
        }

        static uint[,] LoadImage(string path)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            uint[,] result = new uint[image.Width, image.Height];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    result[i, j] = image[i, j].PackedValue;
                }
            }
            return result;
        }

        private static void DisplayAttributes(string attributes, string imageUri)
        {
            Console.WriteLine(imageUri);
            Console.WriteLine(attributes + "\n");
        }
    }
}
