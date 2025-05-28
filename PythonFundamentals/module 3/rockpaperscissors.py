import random

computer_choice = random.choice(['rock', 'paper', 'scissors'])

user_choice = input("Enter rock, paper, or scissors: ").lower()

if user_choice not in ['rock', 'paper', 'scissors']:
    print("Invalid choice. Please try again.")
if user_choice == computer_choice:
    print(f"It's a tie! You both chose {user_choice}.")
elif (user_choice == 'rock' and computer_choice == 'scissors') or \
     (user_choice == 'paper' and computer_choice == 'rock') or \
     (user_choice == 'scissors' and computer_choice == 'paper'):
    print(f"You win! {user_choice} beats {computer_choice}.")
else:
    print(f"You lose! {computer_choice} beats {user_choice}.")