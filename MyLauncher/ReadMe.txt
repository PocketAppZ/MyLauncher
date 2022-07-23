The My Launcher ReadMe file


Introduction
============
My Launcher started as an application that my 98 year-old mother could use to launch a small handful
of applications easily without trying to find them in the Windows Start menu. I made the font sizes
big, the borders colorful, the spacing wide, and limited the things that could be changed from the
main window. As development continued, I made those things customizable and added new features such
as pop-up windows and the system tray menu from the Tray Launcher app. You don't need to be a
nonagenarian to use My Launcher.

A note on terminology. When this document refers to "List" that refers to the items in the main
window or pop-up windows. "Menu" refers to items in the tray menu. This is important to be aware of
because the list items and menu items are kept in separate files and the file formats are different
and are not interchangeable.


Getting Started
===============
When you first start My Launcher, you will see the main window with a single entry for Calculator.
Clicking on the Navigation menu (the hamburger icon at the left end of the colored bar) will bring
up choices for List Maintenance, Menu Maintenance, App Settings, About and Exit. More on these below.
If you wish to import a Tray Launcher menu file, you may want to stop here and read the conversion.txt
file before returning to this document.


Using My Launcher
=================
After adding your apps, folders, etc. using My Launcher is as easy as pointing the mouse and clicking.
You can choose to use the lists in the main window or the tray menu or both.


List Maintenance
================
On the left side of the List Maintenance page you will see a tree view similar to the left side of
File Explorer. This tree view represents the items in the Launcher page. When you click on an item
in the list, the text boxes on the right side of the window are populated with the details of that
list item. To update an item, simply make any changes in the appropriate text box and then press
the tab key or the enter key.

To add a new item, click the New Item button. You will see a pop-up menu with three item types.

  - Normal item. This item type can be an application, a document, a folder or a website.

  - Pop-Up List. This item type is a window that can contain normal items and other pop-up lists.

  - Special Apps. This is a small set of predefined	apps that can added quickly to My Launcher.
	In addition to being commonly used apps, they can also serve as guides to launching apps by
	URI, specifying a custom icon, and using arguments, which are covered later.

An new entry, temporarily named "untitled", except in the case of Special Apps, is added to the
bottom of the list in the tree view. The text boxes on the right will have some instructional
text. That instructional text will be removed when you select that text box. Note that there can
only be one untitled entry.

The Title text box is where you will enter the text that you want to see in the list in the main
window. It can say anything you want, perhaps Aunt Sally's Secret Buttermilk Biscuit Recipe.

The Path or URL text box is where you will enter the complete path to the file, folder
application, or website URL that you wish to open. For documents, folders and applications, you
may use environmental variables in the path. For example %temp% for the temp folder. For website
URLs, begin the URL with either http:\\ or preferably, https:\\.

The Arguments text box is where you can specify optional arguments. For example, it you want to
open Aunt Sally's Secret Buttermilk Biscuit Recipe in notepad, you would enter notepad.exe in the
Path or URL text box and C:\Recipes\SecretBiscuits.txt in the Arguments textbox.

The Icon File text box is where you can specify a custom icon. My Launcher attempts to find a
suitable icon, but if it can't or you would prefer a different icon this is where you can specify
your own. Acceptable image file types are .ico, .png, .bmp, .jpg and .jpeg.

To arrange list items, simply drag an entry to a new position in the list. Normal items and pop-up
items can be dropped on pop-ups create the pop-up windows. When doing so, the drop indicator turns
into an outline. Once a pop-up has items in it a chevron will appear to its left. Use the chevron
to expand the list as you would in File Explorer.

To delete an item from the list, highlight that item in the list on the left side of the page and
then click the Delete Item button. If you attempt to delete a pop-up that has child items, you will
be asked to confirm the deletion.

Items can be updated by first selecting them in the panel on the left and then by updating the fields
on the right side of the maintenance window. Pay attention to the text in the colored area in the top
box, it will change depending on the item type.

Items can be rearranged by dragging & dropping them into a new position in the panel on the left.
"Normal" items can be dropped onto a pop-up to move them into the pop-up. While dragging over a pop-up
the indicator will change to a rectangle. Pop-ups can be expanded by clicking the chevron symbol to the
left.

When you have finished updating the list, click the Save & Close button. Even though you can see
changes in the main window, the list is not actually saved until the Save & Close button has been
clicked or the List Maintenance window is closed.

If you wish to undo and changes, additions or deletions, click the Discard & Close button before
clicking the Save & Close button or closing the window.

There is also a Backup List button which will copy the list file to a location of your choice.


Menu Maintenance
================
The Menu Maintenance window is very similar to the List Maintenance window with a couple of
exceptions. There isn't a text box for an icon file as icons are not available in the menu.
Also, there isn't an option for Special Apps.

To add a new item, click the New Item button. You will see a pop-up menu with three item types.

  - Menu item. This menu item can be an application, a document, a folder or a website.

  - Submenu. This menu item is a sub menu that can contain normal items and other sub-menus.

  - Separator. This is a horizontal line that can be used to delineate sections in the menu.

Items can be rearranged by dragging & dropping them into a new position in the panel on the left.
A Menu item or a Separator can be dropped on a Submenu item. Just like the list items, when dragging
over a submenu the indicator will change to a rectangle. Submenus can be expanded by clicking the
chevron symbol to the left.

See the discussion of List Maintenance above for the other options.


App Settings
============
In the top section you can choose the theme, primary and secondary accents colors and UI size.
You can also set the font weight, spacing and border width of the list items. Finally you can
set the font size in the tray menu.

The middle section allows you to set the title text on the main page.

The bottom section has several options that control how My Launcher works. Hopefully the
descriptions are self explanatory.

Note that these settings, along with widow size and position, are saved when the application
is closed.


About
=====
The About page displays version information and a link to the GitHib repository.


Keys
====
These keyboard shortcuts are available in the main window:

	Ctrl + 1 through Ctrl + 9 = will open the corresponding entry in the main window (or a pop-up window).
	* The 1-9 keys are those located above the QWERTY row and not those located in the number pad.
	Ctrl + Comma =  Shows the Settings dialog.
	Ctrl + L = Opens the List Maintenance window.
	Ctrl + M = Opens the Menu Maintenance window.
	Ctrl + Numpad Plus = Increases size.
	Ctrl + Numpad Minus = Decreases size.
	Ctrl + Shift + M = Changes the theme.
	Ctrl + Shift + P = Changes the primary accent color.
	Ctrl + Shift + S = Changes the secondary accent color.
	Enter = Launches the selected entry.
	Escape = Clears the selection.
	F1 = Shows the About dialog.


Miscellaneous
=============
In the main window and pop-up windows, you can adjust the width of the window so that the horizontal
scroll bar disappears by double clicking on the accent colored bar at the top.

If you see the question mark icon next to a list item it could be that either the file or folder could
not be found or that My Launcher could not extract the icon. The latter can happen with certain UWP apps.
Feel free to add you own icon if you don't want to look at the question mark.

The three-dot icon in the upper right of the main window will open a menu that has options to view the
readme file and to view the log file.

The three-dot icon in the upper right of the maintenance windows will open a menu that has options to
open the data files and to open the app's folder.

In the maintenance windows, the "i" icon will open a pop-up that shows additional information about the
selected item.


Uninstalling
============
To uninstall use the regular Windows add/remove programs feature.


Notices and License
===================
My Launcher was written in C# by Tim Kennedy and requires .Net 6.

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