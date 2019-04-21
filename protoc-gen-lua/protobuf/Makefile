SRC=pb.c

TARGET=pb.so
#CFLAGS=`pkg-config --cflags lua5.1` -std=gnu99
CFLAGS=-I/usr/local/include -std=gnu99
#LDFLAGS=`pkg-config --libs lua5.1`
LDFLAGS=-L/usr/local/lib -llua


all:$(TARGET)

$(TARGET):$(SRC)
	gcc -O2 -fno-common -c $(SRC) $(CFLAGS) -o pb.o
	gcc -bundle -undefined dynamic_lookup -o pb.so pb.o
