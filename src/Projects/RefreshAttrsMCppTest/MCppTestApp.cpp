// MCppTestApp.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace QQn::TurtleBuildUtils;

int main(array<System::String ^> ^args)
{
	switch(args->Length)
	{
	case 0:
	default:
		Console::WriteLine("Usage RefreshAttrsMCppTestApp <assembly> [<keyfile>]");
		break;
	case 1:
	case 2:
		if(AssemblyUtils::RefreshVersionInfoFromAttributes(args[0], (args->Length > 1) ? args[1] : nullptr, nullptr))
			Console::WriteLine("Updated succesfully");
		else
			Console::WriteLine("Updating failed");
		break;
	}
    return 0;
}
