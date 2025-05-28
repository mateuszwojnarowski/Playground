temperature = (int)(input("What is the temperature in degrees Fahrenheit?\n"))
forecast = input("What is the weather forecast? (sunny, rainy, snowy)\n").lower()

# if temperature < 0 or temperature > 40:
#     print("Stay indside!")
# else:
#     print("You can go outside!")



if temperature > 30:
    print("It's too hot!")
    print("Wear shorts and a t-shirt.")
elif temperature < 0:
    print("It's too cold!")
    print("Wear a heavy coat and boots.")
else:
    print("It's just right!")