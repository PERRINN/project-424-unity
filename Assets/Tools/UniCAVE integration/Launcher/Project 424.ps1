# Windows Powershell Launch Script
# Script Generated On Friday, 02 September 2022, 17:14:33
# Setup contains 2 displays and 0 display managers

# Display: LEFT Display - HEAD
If ($env:ComputerName -eq 'EDYSTONE') {
	& '.\Project 424.exe' -screen-fullscreen 0 -popupwindow 
}

# Display: RIGHT Display - SUB
If ($env:ComputerName -eq 'EDYMOUNTAIN') {
	& '.\Project 424.exe' -screen-fullscreen 0 -popupwindow 
}