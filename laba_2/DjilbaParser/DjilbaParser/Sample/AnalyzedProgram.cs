namespace DjilbaParser.Sample
{
    public static class AnalyzedProgram
    {
        public static int ProcessData(List<int> values)
        {
            int total = 0;
            int positive = 0;
            int negative = 0;
            int index = 0;
            for (int i = 0; i < values.Count; i++)
            {
                int value = values[i];
                total += value;
                if (value > 0)
                {
                    positive++;
                }
                else
                {
                    negative++;
                }
            }
            foreach (int value in values)
            {
                if (value == 0)
                {
                    continue;
                }
                switch (Math.Abs(value) % 4)
                {
                    case 0:
                        total += 4;
                        break;
                    case 1:
                        total += 1;
                        if (value > 10)
                        {
                            total += 2;
                        }
                        break;
                    case 2:
                        total += 2;
                        for (int j = 0; j < 2; j++)
                        {
                            total += j;
                        }
                        break;
                    case 3:
                        total += 3;
                        break;
                    default:
                        total--;
                        break;
                }
            }
            while (index < values.Count)
            {
                int current = values[index];
                if (current < -5)
                {
                    total -= 2;
                }
                else if (current == -5)
                {
                    total -= 1;
                }
                else
                {
                    if (current % 2 == 0)
                    {
                        total += 2;
                    }
                    else
                    {
                        total += 1;
                    }
                }
                index++;
            }
            int attempts = 0;
            do
            {
                attempts++;
                if (attempts % 2 == 0)
                {
                    total += attempts;
                }
                else
                {
                    total -= attempts;
                }
            } while (attempts < 3);
            if (positive > negative)
            {
                total += positive;
            }
            else if (positive == negative)
            {
                total += 0;
            }
            else
            {
                total -= negative;
            }
            switch (values.Count % 3)
            {
                case 0:
                    total += 10;
                    break;
                case 1:
                    total += 20;
                    if (total > 100)
                    {
                        total /= 2;
                    }
                    break;
                case 2:
                    total += 30;
                    break;
            }
            return total;
        }
    }
}
