The My Launcher ReadMe file


Introduction
============








	List Maintenance
	----------------
	On the left side of the List Maintenance page you will see a scrollable list. This list represents
	the items in the Launcher page. When you click on an item in the list, the text boxes on the right side
	of the window are populated with the details of that list item. To update an item, simply make any
	changes in the appropriate text box and then press the tab key or the enter key.

	To arrange list items, simply drag an entry to a new position in the list.

	To add a new item, click the New Item button. An new entry, temporarily named "untitled", is added
	to the bottom of the list on the list. The text boxes on the right will have some instructional
	text. That instructional text will be removed when you select that text box.

	The Title text box is where you will enter the text that you want to see in the list in the main
	window. It can say anything you want, perhaps Aunt Sally's Secret Buttermilk Biscuit Recipe.

	The Path or URL text box is where you will enter the complete path to the file, folder
	application, or website URL that you wish to open. For documents, folders and applications, you
	may use environmental variables in the path. For example %temp% for the temp folder. For website
	URLs, begin the URL with either http:\\ or preferably, https:\\.


	To delete an item from the list, highlight that item in the list on the left side of the page and
	then click the Delete Item button.

	When you have finished updating the list, click the Save button. The list will also be saved when
	you navigate away from the List Maintenance page.

	You can revert back to the last saved list by clicking on the Discard Changes button.


	Settings
	--------
	In the top section you can choose the initial view, UI size, theme and color.

	In the middle section you can select the amount of delay between opening successive documents.
	Choosing a longer time may be helpful on slower computers.

	In the bottom section you can choose to have the ### window close after it opens the list
	items that are selected. You can choose to show or not show the check boxes and file type icons. You
	can choose to have the ###s stay on top of other windows and the detail level of the log
	file.

	Note that these settings, along with widow size and position, are saved when the application is closed.

	About
	-----
	The About page displays version information and a link to the GitHib repository.


Keys
====
These keyboard shortcuts are available:


	Ctrl + Comma =  Show the Settings dialog
	Ctrl + M = Change the theme
	Ctrl + Numpad Plus = Increase size
	Ctrl + Numpad Minus = Decrease size

	F1 = Show the About dialog


Uninstalling
============
To uninstall use the regular Windows add/remove programs feature.


Notices and License
===================
My Launcher was written in C# by Tim Kennedy and now requires .Net 6.

My Launcher uses the following icons & packages:

* Material Design in XAML Toolkit https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit

* NLog https://nlog-project.org/

* GongSolutions.WPF.DragDrop https://github.com/punker76/gong-wpf-dragdrop

* Inno Setup was used to create the installer. https://jrsoftware.org/isinfo.php


MIT License
Copyright (c) 2022 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject
to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.