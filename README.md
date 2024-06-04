# Chant Live

## The voice inside your head.

### Description
An app in which the user is able to join the band - in the gig - 'singing trees to life'.
The user can upload their voice to be mixed into a track.
They will be able to get the track back with their voice mixed in with an accompanying unique sonic footprint visualising the user's voice.
A tree will be planted for each voice uploaded.

### Technologies Used:
 - Unity Engine (version 2022.1.7f1)
 - Firebase Unity
 - Human Voice Pitch Detector
 
## How to install/run/use this project.
### Installation and setup instructions.
 1. Clone the project.
 2. Ensure you have Unity Version 2022.1.7f1 installed using the Unity Hub Application.
 3. Add the cloned project as a project within Unity Hub.
 4. Open the project and make sure you set the build target to android so you get the correct window dimensions when you run the project.

### How to run the project.
 1. If you do not have it open already, click on the project to open it, via the list in the Unity Hub application.
 2. Once the project has loaded into the Unity Editor, open the "Splash Screen" scene.
 3. Click play at the top of the Unity Editor to enter Playmode, you will now be running the project.

## A comprehensive summary of all features and assets developed by the 2022/23 research team for the chant-app
### Backend systems
 - GigInfoTemplate Script - this is a class that stores information that is drawn from firebase about a given gig. This information will then be used by the GigInfoButton Script if a user choses to click on a given gig.
 - GigInfoButton Script - This is a class that gets information from a GigInfoTemplate, and then loads the information into the large display panel that allows users to record their chants for a given gig
 - ListAllGigs Script - This is a script that draws information about all the available gigs from firebase. It then stores the information about each gig in a GigInfoTemplate class. The Script contains two main functions - GetAllGig(), which is a remnant from the previous devs, and GetAllGigs, which is the function currently being used.
 - ListChants Script - Similar to ListAllGigs, this script draws information about given gigs from firebase. However, this script then filters these gigs to only display the gigs that the user has previously submitted a chant for
 - RecieveMixes Script - This is a script that displays information to the user about the status of their chant as it relates to eventually recieving a mix. If the mix is ready, then the script will allow the user to listen to it.
 - SendChant Script - This is a script that uploads a chant and a sonic footprint to the firebase database.
 - Utility - A utility class that as is only contains one function - SplitStringAtChar. The purpose of this function is to provide an easy way to parse the information from firebase. As is, when a chant and message is sent to firebase, it is stored in the form of CHANTNAME_MESSAGE, and the function is able to easily separate this into the individual components of chant name or of message
 
### The Main User-facing experiences and systems.
 - SplashScreen Scene - The purpose of this scene is to give a place to load all dependencies for the App before loading into the login scene whilst also giving a little place showing the name of the app.
 - Login Scene - This is the scene where the user will be able to sign up and create a new account for the app or login, entering their information, to then load in to the main scene and use the app.
 - MainMenu Scene - This is the scene where the bulk of the user's time will be spent. When the user enters this scene they are presented with a home page where they are then able to open the navigation menu with a variety of buttons that let them navigate between 6 screens and logout.
 - Chant Live Gigs Page - This is the page where the user will actually be able to choose a gig they wish to 'chant' for and submit their voice to be put into a mix. They are able to choose from a list of gigs that was fetched from the database and once they choose a gig they are given more imformation and are then prompted to record their chant.
 - The Sonic Footprint Page - This page is where the user actually records their chant, however it isn't able to be navigated by the sidebar menu. A reference tone is played to the user to give them an idea of what they should sound like and then they get a countdown displayed to them to when the recording starts. After the recording is finished they then get prompted pay to send a message and upload if they are happy with the chant.
 - My Chant Live Mixes Page - This is the page where the mixes, with the users chants mixed into them, will be shown in a list to the user so that they can then listen to them and play them back.
 - My Chants Page - This is the page where any gigs the user has previously uploaded a chant for will be displayed in a list. When they click on one of the chants the user is shown their unique sonic footprint along with the message they uploaded with their chant along with a button to give them the option to rerecord the chant.
 - Forests Page - This is the page where the forests, in which the trees are planted in for their chants, will be displayed to the user.
 - About Page - This page shows the user information about the chant project and what the App is all about, along with it's mission.
