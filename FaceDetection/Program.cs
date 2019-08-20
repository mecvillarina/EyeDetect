using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			"https://mecodescdn.blob.core.windows.net/dump/7.jpeg";
		//"https://ak3.picdn.net/shutterstock/videos/24915083/thumb/5.jpg";

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
						imageUrl, false, true, faceAttributes);

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

					uint[,] newImage = new uint[leftEyeDiffWidth, leftEyeDiffHeight];

					var origImage = LoadImage("7.jpeg");

					for (int col = 0; col < leftEyeDiffWidth; col++)
					{
						for (int row = 0; row < leftEyeDiffHeight; row++)
						{
							newImage[col, row] = origImage[eyeLeftXOuter + col, eyeLeftYTop + row];
						}
					}

					SaveImage("7.jpg", origImage);

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
			var s = image.MetaData.ExifProfile.Values.FirstOrDefault(x => x.Tag == SixLabors.ImageSharp.MetaData.Profiles.Exif.ExifTag.Orientation);
			image.
			uint[,] result = new uint[image.Width, image.Height];
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					result[i, j] = image[i, j].PackedValue;
				}
			}

			SaveImage("7.jpg", result);
			return result;
		}

		static void SaveImage(string path, uint[,] image)
		{
			int width = image.GetLength(0);
			int height = image.GetLength(1);

			using (Image<Rgba32> result = new Image<Rgba32>(width, height))
			{
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						result[i, j] = new Rgba32(image[i, j]);
					}
				}
				result.SaveAsPng(new FileStream(path, FileMode.Create));
			}
		}

		private static void DisplayAttributes(string attributes, string imageUri)
		{
			Console.WriteLine(imageUri);
			Console.WriteLine(attributes + "\n");
		}
	}
}
