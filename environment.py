import socket
import os
import time
from enum import Enum

class CommandType(Enum):
    REMOVE = "remove"
    RESET = "reset"
    TIMESCALE = "timescale"
    ISFALLEN = "isfallen"
    UNKNOWN = "unknown"

class Environment:
    def __init__(self, host="127.0.0.1", port=25001):
        self.host = host
        self.port = port

    def send_command(self, command):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.connect((self.host, self.port))
            sock.sendall(command.encode("utf-8"))
            response = sock.recv(1024).decode("utf-8")
            return response.strip()

    def reset(self):
        response = self.send_command("reset")
        return response

    def step(self, action, wait_seconds=0.5):
        level, color = action
        command = f"remove {level} {color}"
        response = self.send_command(command)

        # Wait for the specified time before checking if the tower has fallen
        time.sleep(wait_seconds)

        # Check if the tower has fallen
        is_fallen = self.is_fallen()

        # Retrieve the screenshot after performing the action
        screenshot = self.get_screenshot()
        return screenshot, is_fallen

    def set_timescale(self, timescale):
        command = f"timescale {timescale}"
        response = self.send_command(command)
        return response

    def is_fallen(self):
        response = self.send_command("isfallen")
        return response.lower() == "true"

    def get_screenshot(self):
        # Directory where screenshots are saved
        screenshot_dir = os.path.join(os.getcwd(), "Assets", "Screenshots")

        # Find the only PNG file in the directory
        png_files = [f for f in os.listdir(screenshot_dir) if f.endswith('.png')]

        if len(png_files) == 1:
            screenshot_path = os.path.join(screenshot_dir, png_files[0])
            return screenshot_path
        else:
            raise FileNotFoundError(
                "Expected one PNG file in the directory, but found {}".format(
                    len(png_files)))

def main():
    env = Environment()

    while True:
        print("\nWhat action would you like to do?")
        print("1: Reset Environment")
        print("2: Perform Action (Remove Piece)")
        print("3: Set Timescale")
        print("4: Exit")

        choice = input("Enter the number of your choice: ").strip()

        if choice == "1":
            print("Resetting environment...")
            env.reset()
            print("Environment reset.")

        elif choice == "2":
            level = input("Enter the level number: ").strip()
            color = input("Enter the color (y, b, g): ").strip()
            wait_seconds = input("Enter the wait time in seconds (default is 0.5): ").strip()
            if wait_seconds:
                wait_seconds = float(wait_seconds)
            else:
                wait_seconds = 0.5
            action = (level, color)
            print(f"Performing action: remove piece at level {level}, color {color}...")
            screenshot, is_fallen = env.step(action, wait_seconds)
            print(f"Action performed. Screenshot saved at: {screenshot}")
            print(f"Has the tower fallen? {'Yes' if is_fallen else 'No'}")

        elif choice == "3":
            timescale = input("Enter the timescale value (e.g., 1.5): ").strip()
            print(f"Setting timescale to {timescale}...")
            env.set_timescale(timescale)
            print("Timescale set.")

        elif choice == "4":
            print("Exiting...")
            break

        else:
            print("Invalid choice, please try again.")

if __name__ == "__main__":
    main()
