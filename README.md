# GBTIS (Gesture Based Text Input System)

"Writing in the air" - A Guestbook based on Kinect

GBTIS was our 4th year captsone project, which uses microsoft kinect motion tracking, 
and OCR to convert a users hand movements into text

# Setup

The repo provides a solution compatible with microsoft visual studio 2015 and later.
The [Microsoft Kinect SDK](https://developer.microsoft.com/en-us/windows/kinect)
is also required.  Once the SDK is installed simply import the solution
into visual studio and run

# Configuration

Configuration is handled through the App.config file, the following parameters can be configured
* DemoMode (If in Demo Mode the application exits after every user interaction)
* GBTISaaSAddress - The address of the web service deployement
* AuthorizationKey - The Auth key for the web service API

# Web Service
A web service is available for remote storage, and management of guestbooks.  The repo is
available [here](https://github.com/adambatson/gbtisaas)
