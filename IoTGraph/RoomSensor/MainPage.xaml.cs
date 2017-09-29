using GrovePi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RoomSensor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IBuildGroveDevices _deviceFactory = DeviceFactory.Build;
        private DispatcherTimer timer;
        public MainPage()
        {
            this.InitializeComponent();
            Setup();
            
        }

        void Setup()
        {
            var TempSensor = _deviceFactory.DHTTemperatureAndHumiditySensor(Pin.AnalogPin0,GrovePi.Sensors.DHTModel.Dht11);

            var SoundSensor = _deviceFactory.SoundSensor(Pin.AnalogPin2);

            var DistanceSensor = _deviceFactory.UltraSonicSensor(Pin.DigitalPin7);

            var Buzzer = _deviceFactory.Buzzer(Pin.DigitalPin4);

            var LightSensor = _deviceFactory.LightSensor(Pin.AnalogPin1);

            var GreenLed = _deviceFactory.Led(Pin.DigitalPin3);

            var RedLed = _deviceFactory.Led(Pin.DigitalPin2);
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1000);

            this.timer.Start();
            this.timer.Tick += async(a, b) =>
            {
                TempSensor.Measure();
             
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    TxtDistance.Text = $"Jarak : {DistanceSensor.MeasureInCentimeters()} cm";
                    TxtLight.Text = $"Light : {LightSensor.SensorValue()}";
                    TxtSound.Text = $"Sound : {SoundSensor.SensorValue()}";
                    TxtTemp.Text = $"Temp / Humid : {TempSensor.TemperatureInCelsius} C / {TempSensor.Humidity} %";
                    if (LightSensor.SensorValue() < 30)
                    {
                        CallGraphApi("Cek lampu, kok gelap...");
                    }
                    if (SoundSensor.SensorValue() > 350)
                    {
                        CallGraphApi("Cek ruangan, kok berisik banget...");
                    }
                    if (DistanceSensor.MeasureInCentimeters() < 5)
                    {
                        CallGraphApi("Ada yang masuk tuh ke kelas");
                    }
                });
            };

            
        }

        async void CallGraphApi(string Message)
        {
            string Template = @"{
""planId"": ""bQgSOFUxFk2d_aiqaUNf1skAFrJx"",
            ""bucketId"": ""wNEW8j8bpU6pJIHb8UF2GckABaT3"",
            ""title"": ""[JUDUL]"",
  ""assignments"": { }
        }";
            string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFBQmxEcnFmRUZsU2F1aTZ4blJqWDVFcml3SlN0ZWM5ajg0TUdtRW5TSUo2YUVZUmN3Q0hYNm9TcFpSOW5mQnJvb0JkUXpDZ1FiSGxESW95eDZuZkFOdGhVODEyams2dEhjX19EVHFMOXJnQmlBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoiSEhCeUtVLTBEcUFxTVpoNlpGUGQyVldhT3RnIiwia2lkIjoiSEhCeUtVLTBEcUFxTVpoNlpGUGQyVldhT3RnIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lNGE1Y2QzNi1lNThmLTRmOTgtOGExYS03YThlNTQ1ZmM2NWEvIiwiaWF0IjoxNTA2MTM4NzA1LCJuYmYiOjE1MDYxMzg3MDUsImV4cCI6MTUwNjE0MjYwNSwiYWNyIjoiMSIsImFpbyI6IlkyVmdZSEQxM0hqaFVNVVJ6WGNDRWM3bm8zbWl0SXdVSlVJdit6eG15cFgwWjh4b0RBVUEiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IlJvb21TZW5zb3JBcHAiLCJhcHBpZCI6IjNiYjMyNjQ2LTRlN2UtNGVlZC04MzIzLTQ4MDZhM2IxNzMxZSIsImFwcGlkYWNyIjoiMCIsImZhbWlseV9uYW1lIjoiSWJudSBGYWRoaWwiLCJnaXZlbl9uYW1lIjoiTXVoYW1tYWQiLCJpcGFkZHIiOiIyMDIuNjIuMTkuNTMiLCJuYW1lIjoiTXVoYW1tYWQgSWJudSBGYWRoaWwiLCJvaWQiOiJhNjNmYTQwNi1kZjAyLTRlZTItODY1NC0xYzAyNTM3NWI0MzMiLCJwbGF0ZiI6IjUiLCJwdWlkIjoiMTAwM0JGRkQ5RDQ5QjBCNiIsInNjcCI6IkNhbGVuZGFycy5SZWFkIENhbGVuZGFycy5SZWFkLlNoYXJlZCBDYWxlbmRhcnMuUmVhZFdyaXRlIENhbGVuZGFycy5SZWFkV3JpdGUuU2hhcmVkIENvbnRhY3RzLlJlYWQgQ29udGFjdHMuUmVhZC5TaGFyZWQgQ29udGFjdHMuUmVhZFdyaXRlIENvbnRhY3RzLlJlYWRXcml0ZS5TaGFyZWQgRGV2aWNlLkNvbW1hbmQgRGV2aWNlLlJlYWQgRGV2aWNlTWFuYWdlbWVudEFwcHMuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudEFwcHMuUmVhZFdyaXRlLkFsbCBEZXZpY2VNYW5hZ2VtZW50Q29uZmlndXJhdGlvbi5SZWFkLkFsbCBEZXZpY2VNYW5hZ2VtZW50Q29uZmlndXJhdGlvbi5SZWFkV3JpdGUuQWxsIERldmljZU1hbmFnZW1lbnRNYW5hZ2VkRGV2aWNlcy5Qcml2aWxlZ2VkT3BlcmF0aW9ucy5BbGwgRGV2aWNlTWFuYWdlbWVudE1hbmFnZWREZXZpY2VzLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRNYW5hZ2VkRGV2aWNlcy5SZWFkV3JpdGUuQWxsIERldmljZU1hbmFnZW1lbnRSQkFDLlJlYWQuQWxsIERldmljZU1hbmFnZW1lbnRSQkFDLlJlYWRXcml0ZS5BbGwgRGV2aWNlTWFuYWdlbWVudFNlcnZpY2VDb25maWcuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudFNlcnZpY2VDb25maWcuUmVhZFdyaXRlLkFsbCBEaXJlY3RvcnkuQWNjZXNzQXNVc2VyLkFsbCBEaXJlY3RvcnkuUmVhZC5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgRWR1QWRtaW5pc3RyYXRpb24uUmVhZCBFZHVBZG1pbmlzdHJhdGlvbi5SZWFkV3JpdGUgRWR1QXNzaWdubWVudHMuUmVhZCBFZHVBc3NpZ25tZW50cy5SZWFkQmFzaWMgRWR1QXNzaWdubWVudHMuUmVhZFdyaXRlIEVkdUFzc2lnbm1lbnRzLlJlYWRXcml0ZUJhc2ljIEVkdVJvc3Rlci5SZWFkIEVkdVJvc3Rlci5SZWFkQmFzaWMgRWR1Um9zdGVyLlJlYWRXcml0ZSBlbWFpbCBGaWxlcy5SZWFkIEZpbGVzLlJlYWQuQWxsIEZpbGVzLlJlYWQuU2VsZWN0ZWQgRmlsZXMuUmVhZFdyaXRlIEZpbGVzLlJlYWRXcml0ZS5BbGwgRmlsZXMuUmVhZFdyaXRlLkFwcEZvbGRlciBGaWxlcy5SZWFkV3JpdGUuU2VsZWN0ZWQgR3JvdXAuUmVhZC5BbGwgR3JvdXAuUmVhZFdyaXRlLkFsbCBJZGVudGl0eVJpc2tFdmVudC5SZWFkLkFsbCBNYWlsLlJlYWQgTWFpbC5SZWFkLlNoYXJlZCBNYWlsLlJlYWRXcml0ZSBNYWlsLlJlYWRXcml0ZS5TaGFyZWQgTWFpbC5TZW5kIE1haWwuU2VuZC5TaGFyZWQgTWFpbGJveFNldHRpbmdzLlJlYWQgTWFpbGJveFNldHRpbmdzLlJlYWRXcml0ZSBNZW1iZXIuUmVhZC5IaWRkZW4gTm90ZXMuQ3JlYXRlIE5vdGVzLlJlYWQgTm90ZXMuUmVhZC5BbGwgTm90ZXMuUmVhZFdyaXRlIE5vdGVzLlJlYWRXcml0ZS5BbGwgTm90ZXMuUmVhZFdyaXRlLkNyZWF0ZWRCeUFwcCBvZmZsaW5lX2FjY2VzcyBvcGVuaWQgUGVvcGxlLlJlYWQgUGVvcGxlLlJlYWQuQWxsIHByb2ZpbGUgUmVwb3J0cy5SZWFkLkFsbCBTaXRlcy5GdWxsQ29udHJvbC5BbGwgU2l0ZXMuTWFuYWdlLkFsbCBTaXRlcy5SZWFkLkFsbCBTaXRlcy5SZWFkV3JpdGUuQWxsIFRhc2tzLlJlYWQgVGFza3MuUmVhZC5TaGFyZWQgVGFza3MuUmVhZFdyaXRlIFRhc2tzLlJlYWRXcml0ZS5TaGFyZWQgVXNlci5JbnZpdGUuQWxsIFVzZXIuUmVhZCBVc2VyLlJlYWQuQWxsIFVzZXIuUmVhZEJhc2ljLkFsbCBVc2VyLlJlYWRXcml0ZSBVc2VyLlJlYWRXcml0ZS5BbGwgVXNlclRpbWVsaW5lQWN0aXZpdHkuV3JpdGUuQ3JlYXRlZEJ5QXBwIiwic3ViIjoiRnV0Wi10cUdGSGphRFlDWF9HOExvTXFteUw2Rk9aazc3OThhYVN5QlpEZyIsInRpZCI6ImU0YTVjZDM2LWU1OGYtNGY5OC04YTFhLTdhOGU1NDVmYzY1YSIsInVuaXF1ZV9uYW1lIjoiZmFkaGlsQGdyYXZpY29kZXBsZXgub25taWNyb3NvZnQuY29tIiwidXBuIjoiZmFkaGlsQGdyYXZpY29kZXBsZXgub25taWNyb3NvZnQuY29tIiwidXRpIjoiVmpHRDdISFdUa1daTG16UEQ0a0JBQSIsInZlciI6IjEuMCIsIndpZHMiOlsiNjJlOTAzOTQtNjlmNS00MjM3LTkxOTAtMDEyMTc3MTQ1ZTEwIl19.TcfZAYG4ys9n2PhPpoGKCZCz77FLy1gEI49XuD-l2X5SSeImwYCvVJ2GXtvrPpT6tP8aRanaT_3KI8P8v3uc7mxLpGmEs8Py48azdG4TyLewS2Mnincq5gTO5AVHhkfwlJOD_0KWzFUeKKl-HNBzB7Ky5t6Cl6iXVrMtY2nitjSzu-zjVmp9RTJia4O2dB4RIm0mW3tczCagcLN3ejCBbI0IyVtfSWDJrzqwG94wAMAt9WzOoU_ll4RS3hxQEeYyDUDQvvj6p1ggGvlL7Pn5-90Ubj3pYQGfKL_WNLBQTKth2GblM6gn8LMjbAyi7uXDQ4MDLolN7JTAmXrwMerexA";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            //client.DefaultRequestHeaders.Add("Content-type", "application/json");
            //https://graph.microsoft.com/v1.0/users/
            StringContent content = new StringContent(Template.Replace("[JUDUL]", Message),
                Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://graph.microsoft.com/v1.0/planner/tasks",content);
            string retResp = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(retResp);
            Debug.WriteLine($"Call graph api : {Message}");
        }
    }
}
