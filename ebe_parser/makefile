ebe: ebe.c eval.c linked_list.c expression.c helper.c log.c bstring.o md5.o
	gcc -g -o ebe ebe.c eval.c linked_list.c expression.c helper.c log.c md5.o -I.

md5.o: ./md5/md5.c ./md5/md5.h
	gcc -c ./md5/md5.c -I.
