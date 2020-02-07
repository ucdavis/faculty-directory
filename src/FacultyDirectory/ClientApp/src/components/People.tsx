import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ReactTable } from './ReactTable';

export const People = (props: any) => {
  const [people, setPeople] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const getPeople = async () => {
      const results = await fetch('SitePeople').then(r => r.json());
      setPeople(results);
      setLoading(false);
    };

    getPeople();
  }, []);

  if (loading) {
    return <span>loading...</span>;
  }

  // TOOD: start paging when record get large
  const orderedPeople = people.sort((a: any, b: any) =>
    a.person.lastName.localeCompare(b.person.lastName)
  );

  const navLink = ({ row }: any) => {
    const { person } = row.original; // get the original data back for this row
    return <Link to={'/People/' + person.id}>View Record</Link>;
  };

  const decision = ({ sitePerson }: any) => {
    if (sitePerson) {
      return sitePerson.shouldSync ? 'sync' : 'hold';
    } else {
      return 'none';
    }
  }

  const columns = [
    { Header: '', id: 'detail', Cell: navLink },
    { Header: 'First', accessor: 'person.firstName' },
    { Header: 'Last', accessor: 'person.lastName' },
    { Header: 'Decision', accessor: decision }
  ];

  return <ReactTable columns={columns} data={orderedPeople} />;
};
