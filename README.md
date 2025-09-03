![PerfectVision](https://github.com/Saurabh528/perfect-vision-final/assets/149320858/83c17cf2-feaa-4b85-a59f-f8cf6b32152e)

:tada: Welcome to PerfectVision Project :tada:

Perfect Vision revolutionizes amblyopia therapy with seven engaging games designed for use with anaglyph glasses. It turns vision therapy for lazy eye into a personalized, enjoyable experience. With adaptable difficulty and progress tracking, it offers an effective and engaging way to improve visual acuity and depth perception.

## Requirements
To deploy and test PerfectVision on any platform, it is recommended to install High-resolution Web camera.
As well as you need anaglyph glasses.
## Getting Started

This repository contains all kinds of resources for Project from scripts to datasets.

1. You can [browse this repository](#content) and find your desired sources or you can [clone this repository](https://help.github.com/articles/cloning-a-repository/) and browse the files:

```bash
git clone https://github.com/Saurabh528/perfect-vision-final.git
```

then open the folder in unity or you can find useful stuff by [browsing awesome resources in the below](#resources).

2. Install Unity 2021.3.27f1 or higher version.
3. Install [PlayFabSDK](https://github.com/PlayFab/UnitySDK/releases/) on Unity.
4. Install [PlayFab UnityEditor Extension](https://github.com/PlayFab/UnityEditorExtensions/releases/) on Unity.
5. Install Python and necessary modules to run app on Unity Editor.
	```bash
	pip install mediapipe
	```
	and other modules: numpy, pandas, queue
6. You can use [PyCharm](https://www.jetbrains.com/pycharm/download/), [Anaconda](https://www.anaconda.com/download) or other IDE to edit and debug python scripts.



[:sparkles: Contribution is Welcome](#contribution)

[:fire: Join the Community](#join-the-community)


## Content

- [Assets/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Assets) : Unity assets including scripts, textures, audio clips and so on.
- [Design/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Design) : Non-unity assets but necessary to make unity assets, including 3DS Max files.
- [Python/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Python) : Python scripts used in machine learning and computer vision
	◦ [Alignment/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Python/Alignment) : Python scripts used in Alignment tool
	◦ [CoverUnCover/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Python/CoverUnCover) : Python scripts used in CoverUnCover tool
	◦ [Displacement/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Python/Displacement) : Python scripts used in Displacement tool
	◦ [ScreenCali/](https://github.com/Saurabh528/perfect-vision-final/tree/master/Python/ScreenCali) : Python scripts used in ScreenCali tool
- [WindowsBuild/](https://github.com/Saurabh528/perfect-vision-final/tree/master/WindowsBuild) : Executables for Windows platform. This includes main unity project executables and python builds. Python scripts in Python directory can be built using pyinstaller command and .spec files.

# Additions

### Adding new therapy game
- add to SessionMgr.gamenames
- add to SessionMgr.gameScenenames
- increase count Enrollment.unity/...RightView/ScrollView/Mask/PatientView/HightscoreView.UISessionReportView.Gamenames, Maxscores,MaxLevels,MaxavgTimes
- add list item to Enrollment.unity/...RightView/ScrollView/Mask/PatientView/SessionMakeView/GameInfo/Dropdown_AddNewGame.Dropdown
- increase count Enrollment.unity/...RightView/ScrollView/Mask/PatientView/SessionReportView tmpl/UISessionReportView component
- increase count Enrollment.unity/...RightView/ScrollView/Mask/PatientView/HightscoreView/UISessionReportView component
- increase count HomeTherapy.unity/...RightView/ScrollView/Mask/PatientView/HightscoreView/UISessionReportView component
- increase count HomeTherapy.unity/...RightView/ScrollView/Mask/PatientView/ReportView/UISessionReportView component
- add row in UISessionRecordView.cs.OnBtnExportPDF
- add game scene name in StartScene.unity/GlobalCanvas gameobject/BackgoundTask component/Rgb Allowed Scenes variable
- add game scene in build setting

### Gamescore customizing
- UIProgressView.ViewProgression(string gamename)

### Function: Save Data
This function SaveData uses PlayFab's API to update a user's session data with specified x and y coordinates along with the current date. It increments a session count, updates or appends a new session entry in JSON format, and handles both success and error callbacks with appropriate console logging. This method is useful for tracking and storing user position data over multiple sessions in an application.

### KeyValue pair of Diagnostic Tools



## How to Build
	Build on Mac OS
		1. Build Unity project
		2. Create dir "Python" inside PerfectVision.app directory
		3. Create conda environment "Perfect" in Python directory.
			
		4. Copy .py files from ../../../../Python directory to current dir.
		5 install python packages using ../../../../Python/requirements.txt
		6. Create package using PerfectVision.pkgproj.

	Test on Mac OS
		1. Open Terminal.
		2. Run the following command to enable the "Anywhere" option in Gatekeeper:
			sudo spctl --master-disable
		3. Enter your administrator password when prompted.

	Sign on Mac OS
		codesign --deep --verbose --force --options runtime --sign "Developer ID Application: Your Developer Name (Team ID)" /path/to/PerfectVision.app




## Contribution

Any contribution to this repository are welcome.

Also you [join as a member](#join-the-community) to do more stuff such as creating new repositories for more awesome open source works.


## Join the Community

Send your GitHub id to one of the below ways:

- [:speech_balloon: Unity Forums](https://forum.unity3d.com/conversations/add?to=mgear) <sup><i>*Private message</i></sup>
- Signup using [Google Forms](https://goo.gl/forms/DFspn3ByJBoLWEth2) <sup><i>*Can take few days until processed</i></sup>
- [:e-mail: seanchihi@gmail.com](mailto:seanchihi@gmail.com)



## Improvements/Ideas/Feedback

Feel free to [:postbox: Post your ideas/comments/improvements/recommendations](https://github.com/Saurabh528/perfect-vision-final/issues)

While pushing repository I got following errors.

File causing error:
Packages/com.github.homuler.mediapipe/Runtime/Plugins/Android/mediapipe_android.aar (277.62 MB)
Packages/com.github.homuler.mediapipe/Runtime/Plugins/iOS/MediaPipeUnity.framework/MediaPipeUnity (181.78 MB)

To solve abive issues, anyone should m

## Credits

- [TopServicer (NiKuang)](https://github.com/NiKuang413) (Owner)
- [Saurabh](https://github.com/Saurabh528) (Owner)
- [Arpit Jindal](https://github.com/Arpit-Jindal) (Owner)


## License

[None](https://github.com/Saurabh528/perfect-vision-final/LICENSE) @ [Unity Community](https://github.com/UnityCommunity/)
