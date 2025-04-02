#ifndef IO_H
#define IO_H

#include "def.h"

void output_data(result_type *result, parameter_type *parameters);
int input_data(int argc,char *argv[],parameter_type * parameters);
void calculate_time(result_type * result, parameter_type *parameters);

#endif

