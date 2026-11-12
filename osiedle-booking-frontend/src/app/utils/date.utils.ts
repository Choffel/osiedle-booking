import { DateOption } from '../../models/DateOption';

export function generateUpcomingDays(): DateOption[]
{
  const today = new Date();
  const days: DateOption[] = [];

  for (let i = 0; i < 4; i++){
    const targetDate = new Date(today);
    targetDate.setDate(today.getDate() + i);

    if(i === 0){
      days.push({
        isoDate: targetDate.toISOString().split('T')[0],
        dayLabel: 'Dziś',
        dateLabel: targetDate.toLocaleDateString('pl-PL', { day: 'numeric', month: 'long' }),
      });

      continue;
    }

    if (i === 1) {
      days.push({
        isoDate: targetDate.toISOString().split('T')[0],
        dayLabel: 'Jutro',
        dateLabel: targetDate.toLocaleDateString('pl-PL', { day: 'numeric', month: 'long' }),
      });

      continue;
    }

    days.push({
      isoDate: targetDate.toISOString().split('T')[0],
      dayLabel: targetDate.toLocaleDateString('pl-PL', { weekday: 'short' }),
      dateLabel: targetDate.toLocaleDateString('pl-PL', { day: 'numeric', month: 'long' }),
    });
  }

  return days;
}
