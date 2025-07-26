New-CimInstance -ClassName Win32_TCPIPPrinterPort -Property @{
	Name = "PPortName"
	HostAddress = "98.171.192.82"
	PortNumber = [uint32]9100
	Protocol = [uint32]1
	SNMPEnabled = $false
} -Namespace root/cimv2