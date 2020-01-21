import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

export const People = (props: any) => {
  const [people, setPeople] = useState<any[]>([]);
  useEffect(() => {
    const getPeople = async () => {
      setPeople(await fetch('SitePeople').then(r => r.json()));
    };

    getPeople();
  }, []);

  // TOOD: use react table or something better for large lists
  const orderedPeople = people.sort(
    (a: any, b: any) => a.person.lastName.localeCompare(b.person.lastName)
  );

  return (
    <div>
      <ul>
        {orderedPeople.map(p => (
          <li key={p.person.id}>
            <Link to={'/People/' + p.person.id}>
              {p.person.firstName} {p.person.lastName}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
};
