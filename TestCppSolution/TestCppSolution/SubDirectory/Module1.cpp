#include "stdafx.h"
#include "Module1.h"

#include "../../Module2Indirect.h"
#include "../../Module2.h"

#include <iostream>

void Module1::Foo() {
	std::cout << "here" << std::endl;

	Module2 m2;
	m2.Foo();
}