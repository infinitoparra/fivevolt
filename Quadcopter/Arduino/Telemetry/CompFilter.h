
#ifndef _comp_filter_h_
#define _comp_filter_h_

class CompFilter {
public:
  void init(float angle, float rate);
  float update(float newAngle, float newRate);

private:
  float _dt;
  float _previousAngle;
  float _filterTerm0;
  float _filterTerm1;
  float _filterTerm2;
  float _timeConstantCF;
  float _timeConstant;
};

#endif

