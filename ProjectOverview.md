# Introduction #

Our Project involves using Augmented Reality to make a clone of Tower Defense


# Details #

Current Plans:

  * use C# libs to get webcam feed
  * use ARToolkitPlus C# wrapper to detect symbols
  * use XNA for graphical user interface

Steps Taken By Nathan and Perry:
  * Research using ARToolkit.
  * Possible split of 2 applications and networking C++ to c#.
  * Splitting into 2 separate applications proved too difficult. However we found a C# wrapper which is far easier to work with and should work nicely with XNA.
  * Pull apart wrapper and understand internal workings in order to adapt it to our uses.
  * Remove WPF from wpfaugreal example.
  * Write a nice class to interact with the ARWrapper.
  * Recognise Multiple Trackers.
  * Convert matrices into XNA format from GL format.
  * Get camera feed into bitmap buffer or another format that works with AR (into XNA)

Next Steps for Perry and Nathan:
  * Improve AR detection - set a ghost of the object for 50 frames of so.

Steps Taken By Daniel and Leo:
  * Familiarize ourselves with XNA in both 2D and 3D environments.
  * Produce a simple 2D model of our Tower Defense game in XNA.
  * Begin working with 3D models and camera settings etc.

Next Steps for Daniel and Leo:
  * Complete the 2D model of Tower Defense.
  * Transfer the model into 3D.
  * Change structure of game to work with data from AR.