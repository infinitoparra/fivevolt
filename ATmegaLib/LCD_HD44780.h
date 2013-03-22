
#ifndef LCD_HD44780
#define LCD_HD44780

#include <avr/io.h>
#include <util/delay.h>
#include "Print.h"
#include "IO.h"

typedef enum LCD_DataLength_enum {
	LCD_DataLength_4bits = 4,
	LCD_DataLength_8bits = 8
} LCD_DataLength_t;

typedef enum LCD_NumberOfLines_enum {
	LCD_NumberOfLines_1 = 1,
	LCD_NumberOfLines_2 = 2
} LCD_NumberOfLines_t;

typedef enum LCD_FontType_enum {
	LCD_FontType_5x8 = 1,
	LCD_FontType_5x11 = 2
} LCD_FontType_t;

typedef enum LCD_Display_enum {
	LCD_Display_On = 1,
	LCD_Display_Off = 0
} LCD_Display_t;

typedef enum LCD_Cursor_enum {
	LCD_Cursor_On = 1,
	LCD_Cursor_Off = 0
} LCD_Cursor_t;

typedef enum LCD_CursorBlink_enum {
	LCD_CursorBlink_On = 1,
	LCD_CursorBlink_Off = 0
} LCD_CursorBlink_t;

typedef enum LCD_EntryMode_enum {
	LCD_EntryMode_Increment = 1,
	LCD_EntryMode_Decrement = 0
} LCD_EntryMode_t;

typedef enum LCD_EntryModeShift_enum {
	LCD_EntryModeShift_On = 1,
	LCD_EntryModeShift_Off = 0
} LCD_EntryModeShift_t;

typedef enum LCD_Instruction_enum {
	LCD_Instruction_Clear           = (1<<0),
	LCD_Instruction_EntryMode       = (1<<2),
	LCD_Instruction_Display         = (1<<3),
	LCD_Instruction_FunctionSet     = (1<<5),
	LCD_Instruction_SetDDRAMAddress = (1<<7)
} LCD_Instruction_t;

class LCD : public Print {
public:
	LCD(OutputPort &dataPort, OutputPin &enablePin, OutputPin &rsPin, OutputPin *rwPin);
	void begin();
	void sendInstruction(uint8_t v);
	void setDisplay(LCD_Display_t display, LCD_Cursor_t cursor, LCD_CursorBlink_t cursorBlink);
	void setFunction(LCD_DataLength_t dataLength, LCD_NumberOfLines_t numberOfLines, LCD_FontType_t fontType);
	void setEntryMode(LCD_EntryMode_t increment, LCD_EntryModeShift_t shift);
	void setXY(uint8_t x, uint8_t y);
	void clear();
	virtual void write(uint8_t c);

private:
	OutputPort *_dataPort;
	OutputPin *_enablePin;
	OutputPin *_rsPin;
	OutputPin *_rwPin;
};


#endif