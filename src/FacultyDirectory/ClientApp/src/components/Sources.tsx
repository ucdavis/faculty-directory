import React from 'react';

import { ISource } from '../models/ISource';

interface IProps {
  sources: ISource[];
  onChange: (sources: ISource[]) => void;
}

// TODO: find a better way to pull static list of all source types
const sourceTypes = ['scholar'];

export const Sources = (props: IProps) => {
  const changeHandler = (event: any) => {
    const name = event.target.name;
    const value = event.target.value;

    // TODO: this version won't preserve indexes so maybe get smarter about this later
    // get the sources without the one we are changing
    const updatedSources = props.sources.filter(s => s.source !== name);

    // add the changing one back in
    updatedSources.push({ source: name, sourceKey: value });

    props.onChange(updatedSources);
  };

  const renderSources = () => {
    return sourceTypes.map(sourceType => {
      const sourceForType = props.sources.find(s => s.source === sourceType);

      return (
        <div className='sources' key={sourceType}>
          <div className='form-group form-small'>
            <label>{sourceType}</label>
            <input
              type='text'
              className='form-control form-control-sm'
              name={sourceType}
              placeholder='Scholyr ID'
              value={sourceForType?.sourceKey || ''}
              onChange={changeHandler}
            />
          </div>
        </div>
      );
    });
  };

  return (
    <div>
      <br/>
        <p className="mb-0"><b>Source information</b></p>
        <div className='sourceIDs'>{renderSources()}</div>

    </div>
  );
};
