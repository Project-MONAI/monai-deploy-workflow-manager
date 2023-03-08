<!--
  ~ Copyright 2022 MONAI Consortium
  ~
  ~ Licensed under the Apache License, Version 2.0 (the "License");
  ~ you may not use this file except in compliance with the License.
  ~ You may obtain a copy of the License at
  ~
  ~ http://www.apache.org/licenses/LICENSE-2.0
  ~
  ~ Unless required by applicable law or agreed to in writing, software
  ~ distributed under the License is distributed on an "AS IS" BASIS,
  ~ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  ~ See the License for the specific language governing permissions and
  ~ limitations under the License.
-->


# MONAI Deploy Workflow Manager Requirements

## Overview

The MONAI Workflow Manager Workflow spec can have conditional statements this is further documentation around conditional statements. 

### EQUALS Operator '=='
**Examples:**

```'F' == 'F'```  = true

```'F' == 'B'``` = false

### GREATER THAN Operator '>' GREATER OR EQUAL TOO Operator '>=' "=>" 
**Examples:**

```'5' > '1'``` = true

```'5' > '1'``` = false

```'5' >= '5'``` = true

```'5' > 'lucy'``` = will cause an error comparing value that isn't a number

### LESS THAN Operator '<' LESS OR EQUAL TOO Operator '<=' "=<" 
**Examples:**

```'1' < '5'``` = true

```'1' < '5'``` = false

```'5' <= '5'``` = true

```'5' < 'lucy'``` = will cause an error comparing value that isn't a number

### NOT EQUALS Operator '!='
**Examples:**

```'F' != 'F'```  = false

```'F' != 'B'``` = true


### AND Operator 'AND' / OR Operator 'OR'
**Examples:**

The AND/OR Operator allows joining multiple statements together

```'1' < '5' OR 'F' == 'B'``` = Left hand side of the OR statement evaluates to true so result will be true

```'1' < '5' AND 'F' == 'B'``` = Right hand side of the AND statement evaluators to false so both sides of AND statement are false and overall statement is false.

This can be used with brackets also for logical priority...

"('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE' OR 'TRUE' == 'TRUE') OR ('TRUE' == 'TRUE' OR 'TRUE' == 'TRUE')" = this statement will equal true.


### CONTAINS Operator 'CONTAINS' and Arrays '[ ]'
**Examples:**

```'lillie' CONTAINS [“jack”, “lillie”, “neil”]``` = true

```[“jack”, “lillie”, “neil”] contains 'lillie'``` = true

```[“jack”, “lucy”, “neil”] contains 'lillie'``` = false

The item array can be contained on either side of the conditional.

### NOT_CONTAINS Operator 'NOT_CONTAINS'
**Examples:**

```'lillie' NOT_CONTAINS [“jack”, “lillie”, “neil”]``` = false

```[“jack”, “lillie”, “neil”] not_contains 'lillie'``` = true

```[“jack”, “lucy”, “neil”] contains 'lillie'``` = false

The item array can be contained on either side of the conditional.

