
#include "LCD_HD44780.h"
#include <stdlib.h>

LCD::LCD(OutputPort &dataPort, OutputPin &enablePin, OutputPin &rsPin, OutputPin *rwPin) {
	_dataPort = &dataPort;
	_enablePin = &enablePin;
	_rsPin = &rsPin;
	_rwPin = rwPin;
}

void LCD::begin() {
	_dataPort->begin();
	_enablePin->begin();
	_rsPin->begin();
	if(_rwPin != NULL) {
		_rwPin->begin();
		_rwPin->clear();
	}
	
	_delay_ms(20);
	setFunction(LCD_DataLength_8bits, LCD_NumberOfLines_2, LCD_FontType_5x11);
	_delay_ms(5);
	setFunction(LCD_DataLength_8bits, LCD_NumberOfLines_2, LCD_FontType_5x11);
	_delay_ms(1);
	setFunction(LCD_DataLength_8bits, LCD_NumberOfLines_2, LCD_FontType_5x11);
	setFunction(LCD_DataLength_8bits, LCD_NumberOfLines_2, LCD_FontType_5x11);
	setDisplay(LCD_Display_On, LCD_Cursor_Off, LCD_CursorBlink_Off);
	clear();
	setEntryMode(LCD_EntryMode_Increment, LCD_EntryModeShift_Off);
	setDisplay(LCD_Display_On, LCD_Cursor_Off, LCD_CursorBlink_Off);
}

void LCD::setEntryMode(LCD_EntryMode_t increment, LCD_EntryModeShift_t shift) {
	sendInstruction(
		LCD_Instruction_EntryMode
		| (increment == LCD_EntryMode_Increment ? (1<<1) : 0)
		| (shift == LCD_EntryModeShift_On ? (1<<0) : 0)
	);
	_delay_us(40);
}

void LCD::setDisplay(LCD_Display_t display, LCD_Cursor_t cursor, LCD_CursorBlink_t cursorBlink) {
	sendInstruction(
		LCD_Instruction_Display
		| (display == LCD_Display_On ? (1<<2) : 0)
		| (cursor == LCD_Cursor_On ? (1<<1) : 0)
		| (cursorBlink == LCD_CursorBlink_On ? (1<<0) : 0)
	);
	_delay_us(40);
}

void LCD::clear() {
	sendInstruction(LCD_Instruction_Clear);
	_delay_ms(2);
}

void LCD::setFunction(LCD_DataLength_t dataLength, LCD_NumberOfLines_t numberOfLines, LCD_FontType_t fontType) {
	sendInstruction(
		LCD_Instruction_FunctionSet
		| (dataLength == LCD_DataLength_8bits ? (1<<4) : 0)
		| (numberOfLines == LCD_NumberOfLines_2 ? (1<<3) : 0)
		| (fontType == LCD_FontType_5x11 ? (1<<2) : 0)
	);
	_delay_us(40);
}

void LCD::sendInstruction(uint8_t instruction) {
	_rsPin->clear();
	_dataPort->write(instruction);
	_enablePin->set();
	_delay_us(1);
	_enablePin->clear();
}

void LCD::write(uint8_t data) {
	_rsPin->set();
	_dataPort->write(data);
	_enablePin->set();
	_delay_us(10);
	_enablePin->clear();
	_delay_us(45);
}

void LCD::setXY(uint8_t x, uint8_t y) {
	// for lines 3 and 4 of a 4 line display the address is calculated as if the lines
	// existed to the right of lines 1 and 2 respecivly
	if(y >= 2) {
		y -= 2;
		x += 0x14;
	}
	sendInstruction(LCD_Instruction_SetDDRAMAddress | (y * 0x40 + x));
	_delay_us(40);
}
