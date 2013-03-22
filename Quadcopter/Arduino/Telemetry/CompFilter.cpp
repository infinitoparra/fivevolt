
#include "CompFilter.h"

#define INITIAL_DT         0.0005
#define INITIAL_TIME_CONST 50

void CompFilter::init(float angle, float rate) {
  _previousAngle = angle;
  _filterTerm2 = rate;
  _dt = INITIAL_DT;
  _timeConstantCF = _timeConstant = INITIAL_TIME_CONST;
}

float CompFilter::update(float newAngle, float newRate) {
  // Written by RoyLB at http://www.rcgroups.com/forums/showpost.php?p=12082524&postcount=1286    
  _filterTerm0 = (newAngle - _previousAngle) * _timeConstant * _timeConstantCF;
  _filterTerm2 += _filterTerm0 * _dt;
  _filterTerm1 = _filterTerm2 + (newAngle - _previousAngle) * 2 * _timeConstantCF + newRate;
  _previousAngle = (_filterTerm1 * _dt) + _previousAngle;
  return _previousAngle; // This is actually the current angle, but is stored for the next iteration
}




