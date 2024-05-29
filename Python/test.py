import pandas as pd

# Load the CSV file
df = pd.read_csv('collected_metrics.csv')

# Group by position and calculate mean and std
result = df.groupby('Position').agg(['mean', 'std'])

# Save the result as a new CSV file
result.to_csv('result.csv')

print("Result saved to 'result.csv'")