import { useState, useEffect } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTimes, faPlus } from '@fortawesome/free-solid-svg-icons';

interface IProps {
    data: string;
    name: string;
    onChange: any; // TODO: update with real values and extract to use here and in input array
}
export const LinksInputArray = (props: any) => {
  const [values, setValues] = useState<string[]>([]);

  useEffect(() => {
    // update our value array each time the input data changes
    var flatData = props.data.flatMap((d: any) => [d.uri,d.title]);
    setValues(flatData);
  }, [props.data]);

  const setValuesAndNotifyProps = (values: string[]) => {
    setValues(values);

    const valuesArray = values.flatMap((v,i) => {
      if (i % 2 === 0) {
        // even indexes are the links.  Always grab the link if it's not empty
        if (!!v) {
          // include the text column too even if empty
          return [v, values[i+1]];
        }
      }

      return null;
    });

    const value = valuesArray.filter(v => v !== null).join('|');

    const response = {
      target: {
        name: props.name,
        value
      }
    };

    // tell the parent about the new values
    props.onChange && props.onChange(response);
  };

  const onChange = (idx: number, e: any) => {
    // given the changed index and
    const newValues = values.map((val, valIdx) => {
      return idx === valIdx ? e.target.value : val;
    });

    setValuesAndNotifyProps(newValues);
  };

  const onRemove = (idx: number) => {
    const baseIndex = idx * 2;
    setValuesAndNotifyProps(values.filter((_, valIdx) => baseIndex !== valIdx && baseIndex + 1 !== valIdx));
  };

  const onAdd = () => {
      // add two inputs per new website
    setValuesAndNotifyProps([...values, '', '']);
  };

  const numGroups = values.length / 2;

  console.log('link array', numGroups, values);

  return (
    <div className='input-array'>
      {[...Array(numGroups)].map((_, i) => (
        <div className='input-group' key={`group-${i}`}>
          <input
            type='text'
            className='form-control'
            placeholder='URL (ex: https://ucdavis.edu)'
            value={values[i * 2]}
            onChange={e => onChange(i * 2, e)}
          ></input>
          <input
            type='text'
            className='form-control'
            placeholder='Link text'
            value={values[i * 2 + 1]}
            onChange={e => onChange(i * 2 + 1, e)}
          ></input>
          <div className='input-group-append'>
            <button
              type='button'
              className='btn'
              onClick={_ => onRemove(i)}
            >
              <FontAwesomeIcon icon={faTimes} />
            </button>
          </div>
        </div>
      ))}
      <button type='button' onClick={onAdd} className='btn addmore-btn'>
        <FontAwesomeIcon icon={faPlus} size='sm' />
        Add Another Website
      </button>
    </div>
  );
};
