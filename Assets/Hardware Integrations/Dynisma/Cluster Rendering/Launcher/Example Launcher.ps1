# Windows Powershell Launch Script
# Script Generated On Friday, 02 September 2022, 17:45:06
# Setup contains 2 displays and 0 display managers

# Display: LEFT Display - SUB
If ($env:ComputerName -eq 'EDYMOUNTAIN') {
	& '.\Project 424.exe' -popupwindow overrideMachineName SUB_LEFT serverAddress 192.168.41.180
}

# Display: RIGHT Display - HEAD
If ($env:ComputerName -eq 'EDYSTONE') {
	& '.\Project 424.exe' -popupwindow overrideMachineName HEAD_RIGHT
}