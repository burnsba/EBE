#!/bin/bash

test_filename="test_filename";
test_md5="";

pass_count=0;
fail_count=0;
total_test=0;

#0,0, d8eada1de0f744e8f2d11cc5ea02451d
#0,1, 1e7b750959daf9c717bee4112d9a7eec
#1,1, d1c07964f6ec244858f0e21a7a0e7fe9
#0,0,0,0, 2d12ed5677fa1e234f1f0938d3cb2fd8
#0,0,0,1, e8b1e6109bdd6a20460f3c3daefebfa6
#0,0,1,0, c031edfa591249d0646e96d8243c8f1b
#0,0,1,1, ccf4850363197298b34e0b246f7c9b19
#0,1,0,0, 4dd8b40a454ca72e19a09b2bdc0cfbcd
#0,1,1,0, 716903117810795beaf119aed6a40de0
#0,1,1,1, 3faa3f56c6bf9a3729e0027d28c33d68
#1,0,0,0, f3216a8a2e1b547fc99e69a5b17f4324
#1,0,1,1, ebbb8ffa4809a61b9ec184cf50b7f39e
#1,1,0,0, 3e826e9713368d676b6a2119cc7322da
#1,1,0,1, e827b2d1475850a55c8e3bb6bf81affb
#1,1,1,0, ba8cf3db9686a44f59d74728d04a4b4d
#1,1,1,1, 1e68819f09ca1283d0dcb678870983e8

# not a numbers
#n,0,n,0, 15336b10b0e7a7106a3a99b615d8e4da
#n,0,n,1, 3917ca0629810c1f7015444dd03031c1

# -b 2
#0,1,2,3,1,2,3,0,2,3,0,1,3,0,1,2, 403e55fe9bd7551a091946eb3c6ca1a2
#0,0,0,0,0,1,0,1,0,0,2,2,0,1,2,3, 2ce62979a31f20d6e6f3860e135990d8
#0,1,2,3,1,0,3,2,2,3,0,1,3,2,1,0, 4a2e9d50991e66d97ca5fa1d9cc13509
#0,1,2,3,1,1,3,3,2,3,2,3,3,3,3,3, efe0f50c1e5168f55461f0758088e7b9
#0,3,2,1,1,0,3,2,2,1,0,3,3,2,1,0, 0764e0526f26a0562d3d69c8c554ec70
#n,0,0,0,n,1,0,0,n,2,1,0,n,3,1,1, 5d8739df3e67bbdf27d82d5a261708f9
#n,0,0,0,n,0,1,1,n,0,0,2,n,0,1,0, 13e1bae98bd6e8f8d04cb4cce1588e9a

function run_test()
{
	if [ -z $3 ]
	then
	{
		bits=1
	}
	else
	{
		bits=$3
	}
	fi
	
	eval ./ebe "\"$1\"" -o $test_filename -b $bits >/dev/null
	
	total_test=$((total_test + 1))

	test_md5=`grep "md5" $test_filename | awk '{$1=$2=""; print $0}' | sed 's/^ *//g'`

	if [ $test_md5 == $2 ]
	then
	{
		pass_count=$((pass_count + 1))
	}
	else
	{
		fail_count=$((fail_count + 1))
		echo -e '\E[47;31m'"\033[1mTest failed for \"$1\"\033[0m" 
		tput sgr0
		
		echo "Output from failed test: `sed '7q;d' $test_filename`"
	}
	fi
}

# one variable
run_test 'a&a' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a|a' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a+a' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a-a' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a*a' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a^a' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a|~a' "d1c07964f6ec244858f0e21a7a0e7fe9"

# parentheses
run_test '(a&a)' "1e7b750959daf9c717bee4112d9a7eec"
run_test '(a|(a))' "1e7b750959daf9c717bee4112d9a7eec"
run_test '((a)+a)' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test '(a)-(a)' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test '(a)*a' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a^(a)' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a|(~a)' "d1c07964f6ec244858f0e21a7a0e7fe9"

# whitespace
run_test ' a & a ' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a| a ' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a    +a' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a-    a' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test '    a*a' "1e7b750959daf9c717bee4112d9a7eec"
run_test 'a^a    ' "d8eada1de0f744e8f2d11cc5ea02451d"
run_test 'a| ~a' "d1c07964f6ec244858f0e21a7a0e7fe9"

# two variables
run_test 'a&b' "e8b1e6109bdd6a20460f3c3daefebfa6"
run_test 'a|b' "3faa3f56c6bf9a3729e0027d28c33d68"
run_test 'a^b' "716903117810795beaf119aed6a40de0"
run_test 'a+b' "716903117810795beaf119aed6a40de0"
run_test 'a-b' "716903117810795beaf119aed6a40de0"
run_test 'a*b' "e8b1e6109bdd6a20460f3c3daefebfa6"
run_test 'a/b' "3917ca0629810c1f7015444dd03031c1"
run_test 'a%b' "15336b10b0e7a7106a3a99b615d8e4da"
run_test 'a<<b' "c031edfa591249d0646e96d8243c8f1b"
run_test 'a>>b' "c031edfa591249d0646e96d8243c8f1b"
run_test 'a&~b' "c031edfa591249d0646e96d8243c8f1b"
run_test '~a&~b' "f3216a8a2e1b547fc99e69a5b17f4324"
run_test '~a|~b' "ba8cf3db9686a44f59d74728d04a4b4d"
run_test 'a|b|~b' "1e68819f09ca1283d0dcb678870983e8"

# parantheses and negates
run_test 'a&b|a' "ccf4850363197298b34e0b246f7c9b19"
run_test '(a&b)|a' "ccf4850363197298b34e0b246f7c9b19"
run_test 'a&(b|a)' "ccf4850363197298b34e0b246f7c9b19"
run_test '(a&b)|~a' "e827b2d1475850a55c8e3bb6bf81affb"
run_test 'a&(b|~a)' "e8b1e6109bdd6a20460f3c3daefebfa6"
run_test '(a&~b)|a' "ccf4850363197298b34e0b246f7c9b19"
run_test 'a&(~b|a)' "ccf4850363197298b34e0b246f7c9b19"
run_test '(~a&b)|a' "3faa3f56c6bf9a3729e0027d28c33d68"
run_test '~a&(b|a)' "4dd8b40a454ca72e19a09b2bdc0cfbcd"
run_test '~a&b' "4dd8b40a454ca72e19a09b2bdc0cfbcd"
run_test 'a&~b' "c031edfa591249d0646e96d8243c8f1b"
run_test '~a&~b' "f3216a8a2e1b547fc99e69a5b17f4324"
run_test '(~a&b)' "4dd8b40a454ca72e19a09b2bdc0cfbcd"
run_test '(a&~b)' "c031edfa591249d0646e96d8243c8f1b"
run_test '(~a&~b)' "f3216a8a2e1b547fc99e69a5b17f4324"
run_test '~(a&b)' "ba8cf3db9686a44f59d74728d04a4b4d"
run_test '~(~a&b)' "ebbb8ffa4809a61b9ec184cf50b7f39e"
run_test '~(a&~b)' "e827b2d1475850a55c8e3bb6bf81affb"
run_test '~(~a&~b)' "3faa3f56c6bf9a3729e0027d28c33d68"

# -b 2 

run_test 'a&b' "2ce62979a31f20d6e6f3860e135990d8" 2
run_test 'a^b' "4a2e9d50991e66d97ca5fa1d9cc13509" 2
run_test 'a|b' "efe0f50c1e5168f55461f0758088e7b9" 2
run_test 'a+b' "403e55fe9bd7551a091946eb3c6ca1a2" 2
run_test 'a-b' "0764e0526f26a0562d3d69c8c554ec70" 2
run_test 'a/b' "5d8739df3e67bbdf27d82d5a261708f9" 2
run_test 'a%b' "13e1bae98bd6e8f8d04cb4cce1588e9a" 2


echo "pass=$pass_count, fail=$fail_count, total=$total_test"