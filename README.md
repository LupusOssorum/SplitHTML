# SplitHTML

nbsp;nbsp;nbsp;nbsp;Split HTML is a simple command line app program to compile partial html file into html file your browser reads.  This is not a program that runs on the server compiling files before they are sent to the user, rather, this will run on your development computer to create html which you put on your web server.

&nbsp;&nbsp;&nbsp;&nbsp;A simple example of how this can be useful.  If you are developing a simple website with a menu bar the is same of all your pages  you can take the html code for the menu bar and put it in a separate file and merely run the Split HTML compiler to put the one file into all your pages.

<h1>How it works</h1>

nbsp;nbsp;nbsp;nbsp;In order to make this work I invented 2 new file types.  For the main html files you will use the ".splithtml" extension.  These are the files that will be compile to a corresponding ".html" file.  You will then have ".htmlpart" files that will be inserted into places in your ".splithtml" file.  You can also nest ".htmlpart" into other ".htmlpart" files!

nbsp;nbsp;nbsp;nbsp;To tell the Split HTML compiler to insert an ".htmlpart" you will put the special comment ("<!--htmlpart:yourfilename-->").

<h1>Very New</h1>

nbsp;nbsp;nbsp;nbsp;This is very new in development.  And, at the time of writing this I literally just posted this up on Github (:.  It works great but I am not finished with sharing all the how tos.  If you found this and would like to give it a shot I am willing to explain how to set it up (or if you have any feedback) feel free to contact me with my email (JonathanILevi@gmail.com) or add an issue.    

I also don't have all the features I think this can have so if you know any C# go ahead and get you feet wet.
